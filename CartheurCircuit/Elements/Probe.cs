namespace CartheurCircuit.Elements
{
    public class Probe : CircuitElement
    {
        public Lead LeadIn { get { return LeadZero; } }
        public Lead LeadOut { get { return LeadOne; } }

        public override void GetInfo(string[] arr)
        {
            arr[0] = "scope probe";
            arr[1] = "Vd = " + CircuitUtilities.GetVoltageText(CircuitUtilities.GetVoltageDifference().ToString());
        }

        public override bool LeadsAreConnected(int n1, int n2)
        {
            return false;
        }

    }
}