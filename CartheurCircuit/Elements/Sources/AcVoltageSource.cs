namespace CartheurCircuit.Elements.Sources
{
    public class AcVoltageSource : Voltage
    {
        public Lead leadPos { get { return LeadZero; } }
        public Lead leadNeg { get { return LeadOne; } }

        public AcVoltageSource() : base(WaveType.AC)
        {

        }

    }
}
