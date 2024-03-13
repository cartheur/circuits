namespace CartheurCircuit
{
    public interface ICircuitElement
    {
        void BeginStep(Circuit simulation);
        void Step(Circuit simulation);
        void Stamp(Circuit simulation);

        void Reset();

        int GetLeadNode(int leadNdx);
        void SetLeadNode(int leadNdx, int nodeNdx);

        // lead count
        int GetLeadCount();
        int GetInternalLeadCount();

        // lead voltage
        double GetLeadVoltage(int leadX);
        void SetLeadVoltage(int leadX, double vValue);

        // current
        double GetCurrent();
        void SetCurrent(int voltageSourceNdx, double cValue);

        // voltage
        double GetVoltageDelta();
        int GetVoltageSourceCount();
        void SetVoltageSource(int leadX, int voltageSourceNdx);

        // connection
        bool LeadsAreConnected(int leadX, int leadY);
        bool LeadIsGround(int leadX);

        // state
        bool IsWire();
        bool NonLinear();

    }

    public static class CircuitComponentExtensions
    {
        public static ScopeFrame GetScopeFrame(this ICircuitElement elem, double time)
        {
            return new ScopeFrame
            {
                Time = time,
                Current = elem.GetCurrent(),
                Voltage = elem.GetVoltageDelta(),
            };
        }

        public static string GetCurrentString(this ICircuitElement element)
        {
            return SiUnits.Current(element.GetCurrent());
        }

        public static string GetVoltageString(this ICircuitElement element)
        {
            return SiUnits.Voltage(element.GetVoltageDelta());
        }
    }
}
