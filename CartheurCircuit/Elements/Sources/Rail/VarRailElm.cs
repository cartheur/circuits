namespace CartheurCircuit.Elements.Sources.Rail
{
    public class VarRailElm : VoltageInput
    {
        public double Output { get; set; }

        public VarRailElm()
            : base(WaveType.VAR)
        {
            Output = 1;
            Frequency = MaxVoltage;
            Waveform = WaveType.VAR;
        }

        protected override double GetVoltage(Circuit sim)
        {
            Frequency = Output * (MaxVoltage - Bias) + Bias;
            return base.GetVoltage(sim);
        }
    }
}
