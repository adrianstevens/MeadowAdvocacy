using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnchorMinder
{
    // Simulates a GPS receiver starting at a fixed position.
    // Press button to toggle drift mode — drifts at ~1m/s to trigger anchor alarm.
    // Swap this class for a real Neo8GpsSource when hardware is available.
    public class SimulatedGpsSource : IGpsSource
    {
        public event EventHandler<GeoPosition>? PositionUpdated;

        // Gabriola Island anchorage area
        const double StartLatitude = 49.1250;
        const double StartLongitude = -123.8200;

        // ~1 metre per second drift in degrees
        const double DriftRatePerSecond = 0.000009;
        const int UpdateIntervalMs = 2000;

        double _currentLat = StartLatitude;
        double _currentLon = StartLongitude;
        bool _isDrifting = false;
        CancellationTokenSource? _cts;

        readonly Random _rng = new Random();

        public bool IsDrifting => _isDrifting;

        public void ToggleDrift() => _isDrifting = !_isDrifting;

        public void ResetPosition()
        {
            _currentLat = StartLatitude;
            _currentLon = StartLongitude;
            _isDrifting = false;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => UpdateLoop(_cts.Token));
        }

        public void Stop() => _cts?.Cancel();

        async Task UpdateLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (_isDrifting)
                {
                    var driftPerUpdate = DriftRatePerSecond * (UpdateIntervalMs / 1000.0);
                    _currentLat += driftPerUpdate;
                    _currentLon += driftPerUpdate;
                }
                else
                {
                    // small random jitter to simulate GPS noise
                    _currentLat += (_rng.NextDouble() - 0.5) * 0.000002;
                    _currentLon += (_rng.NextDouble() - 0.5) * 0.000002;
                }

                PositionUpdated?.Invoke(this, new GeoPosition(_currentLat, _currentLon));

                await Task.Delay(UpdateIntervalMs, ct).ContinueWith(_ => { });
            }
        }
    }
}
