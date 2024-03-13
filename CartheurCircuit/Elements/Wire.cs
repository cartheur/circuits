namespace CartheurCircuit.Elements
{
    public class Wire : CircuitElement
    {
        private double current;

        public Lead LeadInput { get { return LeadZero; } }
        public Lead LeadOutput { get { return LeadOne; } }

        public string[] LeadVoltage { get; private set; }

        public override void Stamp(Circuit simulation)
        {
            simulation.StampVoltageSource(LeadNode[0], LeadNode[1], VoltageSource, 0);
        }

        public override int GetVoltageSourceCount()
        {
            return 1;
        }

        public override void GetInfo(string[] arr)
        {
            arr[0] = "wire";
            arr[1] = "I = " + CircuitUtilities.GetCurrentText(current);
            arr[2] = "V = " + CircuitUtilities.GetVoltageText(LeadVoltage[0]);
        }

        public override double GetPower() {
            return 0;
        }

        public override double GetVoltageDelta()
        {
            return VoltageLead[0];
        }

        public override bool IsWire()
        {
            return true;
        }

    }
}
