namespace CartheurCircuit
{
    public class AndGate : LogicGate
    {

        public override bool calcFunction()
        {
            bool f = true;
            for (int i = 0; i != InputCount; i++)
                f &= GetInput(i);
            return f;
        }

    }
}