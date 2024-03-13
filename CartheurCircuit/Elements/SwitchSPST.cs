namespace CartheurCircuit.Elements
{
    public class SwitchSpst : CircuitElement
    {
        public Lead LeadA { get { return LeadZero; } }
        public Lead LeadB { get { return LeadOne; } }
        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>
        /// The position: position 0 == closed, position 1 == open.
        /// </value>
        public int Position { get; private set; }

        public bool IsOpen { get { return Position == 1; } }

        protected int PositionCount;

        public SwitchSpst()
        {
            Position = 0;
            PositionCount = 2;
        }

        public SwitchSpst(bool mm)
        {
            Position = (mm) ? 1 : 0;
            PositionCount = 2;
        }

        public virtual void Toggle()
        {
            Position++;
            if (Position >= PositionCount)
                Position = 0;
        }

        public virtual void SetPosition(int pos)
        {
            Position = pos;
            if (Position >= PositionCount)
                Position = 0;
        }

        public override void CalculateCurrent()
        {
            if (Position == 1)
                Current = 0;
        }

        public override void Stamp(Circuit simulation)
        {
            if (Position == 0)
                simulation.StampVoltageSource(LeadNode[0], LeadNode[1], VoltageSource, 0);
        }

        public override int GetVoltageSourceCount()
        {
            return (Position == 1) ? 0 : 1;
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = string.Empty;
            if(position == 1) {
                arr[1] = "open";
                arr[2] = "Vd = " + getVoltageDText(getVoltageDiff());
            } else {
                arr[1] = "closed";
                arr[2] = "V = " + getVoltageText(LeadVoltage[0]);
                arr[3] = "I = " + getCurrentDText(current);
            }
        }*/

        public override bool LeadsAreConnected(int n1, int n2)
        {
            return Position == 0;
        }

        public override bool IsWire()
        {
            return true;
        }

    }
}