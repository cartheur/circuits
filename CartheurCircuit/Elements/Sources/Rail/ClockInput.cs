namespace CartheurCircuit.Elements.Sources.Rail
{
    public class ClockInput : VoltageInput
    {
        public ClockInput()
            : base(WaveType.SQUARE)
        {
            MaxVoltage = 2.5;
            Bias = 2.5;
            Frequency = 100;
        }

    }
}
