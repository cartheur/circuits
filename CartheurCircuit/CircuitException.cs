namespace CartheurCircuit
{
    public class CircuitException : System.Exception
    {
        public ICircuitElement Element { get; private set; }

        public CircuitException(string why, ICircuitElement elem)
            : base(why)
        {
            Element = elem;
        }
    }
}
