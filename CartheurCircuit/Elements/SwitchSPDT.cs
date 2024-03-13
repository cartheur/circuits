namespace CartheurCircuit.Elements
{
    public class SwitchSpdt : SwitchSpst
    {
        public Lead LeadC { get { return LeadOne; } }

        public SwitchSpdt()
        {
            PositionCount = 3;
        }

        public override int GetLeadCount()
        {
            return PositionCount;
        }

        public override void CalculateCurrent()
        {
            if (Position == 2)
                Current = 0;
        }

        public override void Stamp(Circuit simulation)
        {
            if (Position == 2) return; // in center?
            simulation.StampVoltageSource(LeadNode[0], LeadNode[Position + 1], VoltageSource, 0);
        }

        public override int GetVoltageSourceCount()
        {
            return (Position == 2) ? 0 : 1;
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = "switch (SPDT)";
            arr[1] = "I = " + CircuitElement.getCurrentDText(getCurrent());
        }*/

        public override bool LeadsAreConnected(int leadX, int leadY)
        {
            if (Position == 2) return false;
            return ComparePair(leadX, leadY, 0, 1 + Position);
        }
    }
}
