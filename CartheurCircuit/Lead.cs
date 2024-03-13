using System;

namespace CartheurCircuit
{
    public class Lead
    {
        public ICircuitElement Element { get; private set; }
        public int Nodedx { get; private set; }

        public Lead(ICircuitElement e, int i)
        {
            Element = e; Nodedx = i;
        }
    }
}
