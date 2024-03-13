namespace CartheurCircuit {

	public class Output : CircuitElement {

		public Lead leadIn { get { return LeadZero; } }

		public override int GetLeadCount() {
			return 1;
		}

		public override double GetVoltageDelta() {
			return VoltageLead[0];
		}

	}
}
