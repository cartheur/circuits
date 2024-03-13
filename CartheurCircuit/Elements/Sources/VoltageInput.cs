namespace CartheurCircuit.Elements.Sources
{
    public class VoltageInput : Voltage
    {
        public Lead LeadPositive { get { return LeadZero; } }

        public VoltageInput()
            : base(WaveType.DC)
        {

        }

        public VoltageInput(WaveType wf)
            : base(wf)
        {

        }

        public override int GetLeadCount()
        {
            return 1;
        }

        public override double GetVoltageDelta()
        {
            return VoltageLead[0];
        }

        public override void Stamp(Circuit simulation)
        {
            if (Waveform == WaveType.DC)
            {
                simulation.StampVoltageSource(0, LeadNode[0], VoltageSource, GetVoltage(simulation));
            }
            else
            {
                simulation.StampVoltageSource(0, LeadNode[0], VoltageSource);
            }
        }

        public override void Step(Circuit simulation)
        {
            if (Waveform != WaveType.DC)
                simulation.UpdateVoltageSource(0, LeadNode[0], VoltageSource, GetVoltage(simulation));
        }

        public override bool LeadIsGround(int n1)
        {
            return true;
        }

    }
}