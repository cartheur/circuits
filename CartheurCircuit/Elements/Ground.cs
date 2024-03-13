namespace CartheurCircuit {

	public class Ground : Output {

		public Ground() : base() {

		}

		public override void SetCurrent(int voltageSourceNdx, double c) {
			Current = -c;
		}

		public override void Stamp(Circuit simulation) {
			simulation.StampVoltageSource(0, LeadNode[0], VoltageSource, 0);
		}

		public override double GetVoltageDelta() {
			return 0;
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

		public override bool LeadIsGround(int n1) {
			return true;
		}

	}
}