using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion;

public class RadarTarget
{
    public RadarTarget(int x_mm, int y_mm, int speed_cms, ushort distance_mm)
    {
        X_mm = x_mm;
        Y_mm = y_mm;
        Speed_cms = speed_cms;
        Distance_mm = distance_mm;
    }

    public int X_mm;   // signed mm
    public int Y_mm;   // signed mm
    public int Speed_cms; // signed cm/s
    public ushort Distance_mm; // protocol raw "distance" (unsigned)
}

/// <summary>
/// RD-03D Doppler radar (binary stream: header 0xAA,0xFF,0x03,0x00; footer 0x55,0xCC).
/// Emits up to 3 targets per frame.
/// </summary>
public sealed class Rd03d : IDisposable
{
    // protocol constants
    private static readonly byte[] Header = { 0xAA, 0xFF, 0x03, 0x00 };
    private static readonly byte[] Footer = { 0x55, 0xCC };
    private const int TargetsPerFrame = 3;
    private const int TargetSize = 8; // bytes
    private const int FrameSize = 4 + (TargetsPerFrame * TargetSize) + 2; // 30 bytes

    // serial
    private readonly ISerialPort _port;
    private readonly bool _createdPort;
    private readonly byte[] _buf = new byte[256]; // temp read buffer
    private readonly List<byte> _acc = new List<byte>(128); // accumulator

    private CancellationTokenSource? _cts;
    private Task? _rxTask;

    /// <summary>
    /// Fired whenever a full frame is parsed.
    /// </summary>
    public event EventHandler<IReadOnlyList<RadarTarget>>? TargetsUpdated;

    /// <summary>
    /// Construct using a Meadow-created serial port (recommended for binary data).
    /// </summary>
    public Rd03d(IMeadowDevice device, SerialPortName portName, int baudRate = 256000)
    {
        _port = device.CreateSerialPort(portName, baudRate: baudRate, dataBits: 8, parity: Parity.None, stopBits: StopBits.One);
        _createdPort = true;
    }

    /// <summary>
    /// Construct with an existing ISerialPort you manage.
    /// </summary>
    public Rd03d(ISerialPort port)
    {
        _port = port ?? throw new ArgumentNullException(nameof(port));
    }

    /// <summary>
    /// Open port and start background receive loop.
    /// </summary>
    public void Start()
    {
        if (!_port.IsOpen) _port.Open();
        if (_rxTask != null) return;

        _cts = new CancellationTokenSource();
        _rxTask = Task.Run(() => RxLoop(_cts.Token));
    }

    /// <summary>
    /// Stop background receive loop (port is left open).
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _rxTask = null;
        _cts = null;
    }

    public void Dispose()
    {
        Stop();
        if (_createdPort && _port.IsOpen) _port.Close();
        _port.Dispose();
    }

    // === Receive & parse ===

    private async Task RxLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var n = _port.Read(_buf, 0, _buf.Length);
                if (n > 0)
                {
                    AppendAndParse(_buf, n);
                }
                else
                {
                    await Task.Delay(1, ct); // yield
                }
            }
            catch (OperationCanceledException) { /* normal */ }
            catch (Exception)
            {
                // Swallow or surface via an Error event if you prefer
                await Task.Delay(5, ct);
            }
        }
    }

    private void AppendAndParse(byte[] incoming, int count)
    {
        // accumulate
        for (int i = 0; i < count; i++) _acc.Add(incoming[i]);

        // simple scan; keep buffer from growing unbounded
        // find header positions and attempt to parse fixed-size frames
        int idx = 0;
        while (idx + FrameSize <= _acc.Count)
        {
            if (IsHeaderAt(idx))
            {
                int frameStart = idx;
                int footerStart = frameStart + 4 + TargetsPerFrame * TargetSize;
                if (_acc[footerStart] == Footer[0] && _acc[footerStart + 1] == Footer[1])
                {
                    // parse frame
                    var targets = ParseTargets(frameStart + 4);
                    TargetsUpdated?.Invoke(this, targets);
                    idx += FrameSize; // advance past this frame
                    continue;
                }
            }
            idx++; // slide window
        }

        // drop consumed bytes to keep accumulator small
        if (idx > 0)
        {
            _acc.RemoveRange(0, idx);
        }

        // prevent runaway growth if stream gets desynced
        const int maxAcc = 1024;
        if (_acc.Count > maxAcc)
        {
            // keep last window smaller than a frame to re-sync
            _acc.RemoveRange(0, _acc.Count - FrameSize);
        }
    }

    private bool IsHeaderAt(int i)
    {
        return _acc[i] == Header[0] &&
               _acc[i + 1] == Header[1] &&
               _acc[i + 2] == Header[2] &&
               _acc[i + 3] == Header[3];
    }

    private RadarTarget[] ParseTargets(int dataStart)
    {
        var result = new List<RadarTarget>(TargetsPerFrame);
        for (int t = 0; t < TargetsPerFrame; t++)
        {
            int off = dataStart + t * TargetSize;
            // 8 bytes little-endian: x(2), y(2), speed(2), distance(2)
            ushort xRaw = (ushort)(_acc[off + 0] | (_acc[off + 1] << 8));
            ushort yRaw = (ushort)(_acc[off + 2] | (_acc[off + 3] << 8));
            ushort sRaw = (ushort)(_acc[off + 4] | (_acc[off + 5] << 8));
            ushort dRaw = (ushort)(_acc[off + 6] | (_acc[off + 7] << 8));

            // skip all-zero target
            if (xRaw == 0 && yRaw == 0 && sRaw == 0 && dRaw == 0) continue;

            int x = DecodeSigned15(xRaw);
            int y = DecodeSigned15(yRaw);
            int speed = DecodeSigned15(sRaw); // cm/s
            ushort dist = dRaw;               // protocol appears unsigned

            result.Add(new RadarTarget(x, y, speed, dist));
        }
        return result.ToArray();
    }

    /// <summary>
    /// Protocol: MSB=1 means positive; MSB=0 means negative.
    /// Lower 15 bits are magnitude.
    /// </summary>
    private static int DecodeSigned15(ushort raw)
    {
        bool isPositive = (raw & 0x8000) != 0;
        int mag = raw & 0x7FFF;
        return isPositive ? mag : -mag;
    }
}
