namespace CartheurCircuit
{

    public class AnalogSwitch : CircuitElement
    {
        private double resistance;

        public Lead LeadIn { get { return LeadZero; } }
        public Lead LeadOut { get { return LeadOne; } }
        public Lead LeadSwitch { get { return new Lead(this, 2); } }

        /// <summary>
        /// Normally closed
        /// </summary>
        public bool invert { get; set; }

        /// <summary>
        /// On Resistance (ohms)
        /// </summary>
        public double OnResistance { get; set; }

        /// <summary>
        /// Off Resistance (ohms)
        /// </summary>
        public double OffResistance { get; set; }

        public bool IsOpen { get; protected set; }

        public AnalogSwitch() : base()
        {
            OnResistance = 20;
            OffResistance = 1E10;
        }

        public override void CalculateCurrent()
        {
            Current = (VoltageLead[0] - VoltageLead[1]) / resistance;
        }

        // we need this to be able to change the matrix for each step
        public override bool NonLinear() { return true; }

        public override void Stamp(Circuit simulation)
        {
            simulation.StampNonLinear(LeadNode[0]);
            simulation.StampNonLinear(LeadNode[1]);
        }

        public override void Step(Circuit simulation)
        {
            IsOpen = (VoltageLead[2] < 2.5);
            if (invert) IsOpen = !IsOpen;
            resistance = (IsOpen) ? OffResistance : OnResistance;
            simulation.StampResistor(LeadNode[0], LeadNode[1], resistance);
        }

        public override int GetLeadCount()
        {
            return 3;
        }

        /*public override void getInfo(String[] arr) {
			arr[0] = "analog switch";
			arr[1] = open ? "open" : "closed";
			arr[2] = "Vd = " + getVoltageDText(getVoltageDiff());
			arr[3] = "I = " + getCurrentDText(current);
			arr[4] = "Vc = " + getVoltageText(LeadVoltage[2]);
		}*/

        // we have to just assume current will flow either way, even though that
        // might cause singular matrix errors
        public override bool LeadsAreConnected(int n1, int n2)
        {
            return !(n1 == 2 || n2 == 2);
        }

    }
}
