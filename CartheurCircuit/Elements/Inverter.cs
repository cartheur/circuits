using System;

namespace CartheurCircuit {

	public class Inverter : CircuitElement {

		public Lead leadIn { get { return LeadZero; } }
		public Lead leadOut { get { return LeadOne; } }

		/// <summary>
		/// Slew Rate (V/ns)
		/// </summary>
		public double slewRate { get; set; }

		public Inverter() : base() {
			slewRate = 0.5;
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

		public override void Stamp(Circuit simulation) {
			simulation.StampVoltageSource(0, LeadNode[1], VoltageSource);
		}

		public override void Step(Circuit simulation) {
			double v0 = VoltageLead[1];
			double @out = VoltageLead[0] > 2.5 ? 0 : 5;
			double maxStep = slewRate * simulation.TimeStep * 1e9;
			@out = Math.Max(Math.Min(v0 + maxStep, @out), v0 - maxStep);
			simulation.UpdateVoltageSource(0, LeadNode[1], VoltageSource, @out);
		}

		public override double GetVoltageDelta() {
			return VoltageLead[0];
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "inverter";
			arr[1] = "Vi = " + getVoltageText(LeadVoltage[0]);
			arr[2] = "Vo = " + getVoltageText(LeadVoltage[1]);
		}*/

		// There is no current path through the inverter input, but there
		// is an indirect path through the output to ground.
		public override bool LeadsAreConnected(int n1, int n2) {
			return false;
		}

		public override bool LeadIsGround(int n1) {
			return (n1 == 1);
		}

	}
}