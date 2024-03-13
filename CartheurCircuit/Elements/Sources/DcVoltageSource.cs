namespace CartheurCircuit.Elements.Sources
{
    public class DcVoltageSource : Voltage
    {
        public Lead LeadPositive { get { return LeadZero; } }
        public Lead LeadNegative { get { return LeadOne; } }

        public DcVoltageSource()
            : base(Voltage.WaveType.DC)
        { }

    }
}
