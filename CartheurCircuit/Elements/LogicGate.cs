namespace CartheurCircuit
{
    public abstract class LogicGate : CircuitElement
    {

        public enum Op
        {
            Or,
            And,
            Nor,
            Nand,
            Xor,
        }

        protected int _inputCount;
        public Lead LeadOut { get { return new Lead(this, InputCount); } }

        public Op LogicOp { get; set; }

        /// <summary>
        /// Number of inputs
        /// </summary>
        public int InputCount
        {
            get
            {
                return _inputCount;
            }
            set
            {
                _inputCount = value;
                AllocateLeads();
            }
        }

        public LogicGate() : base()
        {
            InputCount = 2;
        }

        public abstract bool calcFunction();

        public virtual bool isInverting()
        {
            return false;
        }

        public override int GetLeadCount()
        {
            return InputCount + 1;
        }

        public override int GetVoltageSourceCount()
        {
            return 1;
        }

        public override void Stamp(Circuit simulation)
        {
            simulation.StampVoltageSource(0, LeadNode[InputCount], VoltageSource);
        }

        public bool GetInput(int x)
        {
            return VoltageLead[x] > 2.5;
        }

        public override void Step(Circuit simulation)
        {
            bool f = calcFunction();
            if (isInverting()) f = !f;
            simulation.UpdateVoltageSource(0, LeadNode[InputCount], VoltageSource, f ? 5 : 0);
        }

        // There is no current path through the gate inputs, but there is an indirect path through the output to ground.
        public override bool LeadsAreConnected(int n1, int n2)
        {
            return false;
        }

        public override bool LeadIsGround(int n1)
        {
            return (n1 == InputCount);
        }
    }
}
