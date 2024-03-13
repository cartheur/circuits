namespace CartheurCircuit {

	public class OrGate : LogicGate {

		public override bool calcFunction() {
			bool f = false;
			for(int i = 0; i != InputCount; i++)
				f |= GetInput(i);
			return f;
		}

	}
}