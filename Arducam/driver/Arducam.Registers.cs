namespace Meadow.Foundation.Sensors.Camera;

public partial class ArducamBase
{
    protected internal struct SensorReg
    {
        public byte Register;
        public byte Value;

        public SensorReg(byte register, byte value)
        {
            Register = register;
            Value = value;
        }
    }
}