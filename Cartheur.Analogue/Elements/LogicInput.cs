namespace SharpCircuit
{
    public class LogicInput : SwitchSpst
    {
        public Lead LeadOutput { get { return LeadZero; } }

        public double HighVoltage { get; set; }
        public double LowVoltage { get; set; }
        public bool IsTernary { get; set; }
        //public bool isNumeric { get; set; }

        public LogicInput()
        {
            HighVoltage = 5;
            LowVoltage = 0;
        }

        public override int GetLeadCount()
        {
            return 1;
        }

        public override void SetCurrent(int voltageSourceNdx, double c)
        {
            Current = -c;
        }

        public override void Stamp(Circuit simulation)
        {
            double v = (Position == 0) ? LowVoltage : HighVoltage;
            if (IsTernary) v = Position * 2.5;
            simulation.StampVoltageSource(0, LeadNode[0], VoltageSource, v);
        }

        public override int GetVoltageSourceCount()
        {
            return 1;
        }

        public override double GetVoltageDelta()
        {
            return VoltageLead[0];
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = "logic input";
            arr[1] = (position == 0) ? "low" : "high";
            if(isNumeric) arr[1] = "" + position;
            arr[1] += " (" + getVoltageText(LeadVoltage[0]) + ")";
            arr[2] = "I = " + getCurrentText(current);
        }*/

        public override bool LeadIsGround(int n1)
        {
            return true;
        }

    }
}