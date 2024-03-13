using System;

namespace CartheurCircuit {

	// Contributed by Edward Calver.

	public class InvertingSchmittTrigger : CircuitElement {

		public Lead leadIn { get { return LeadZero; } }
		public Lead leadOut { get { return LeadOne; } }

		/// <summary>
		/// Slew Rate (V/ns)
		/// </summary>
		public double slewRate { get; private set; }

		/// <summary>
		/// Lower threshold (V)
		/// </summary>
		public double lowerTrigger { get; private set; }

		/// <summary>
		/// Upper threshold (V)
		/// </summary>
		public double upperTrigger { get; private set; }

		protected bool state;

		public InvertingSchmittTrigger() : base() {
			slewRate = 0.5;
			state = false;
			lowerTrigger = 1.66;
			upperTrigger = 3.33;
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

		public override void Stamp(Circuit simulation) {
			simulation.StampVoltageSource(0, LeadNode[1], VoltageSource);
		}

		public override void Step(Circuit simulation) {
			double v0 = VoltageLead[1];
			double @out;
			if(state) {
				// Output is high
				if(VoltageLead[0] > upperTrigger) {
					// Input voltage high enough to set output low
					state = false;
					@out = 0;
				} else {
					@out = 5;
				}
			} else {
				// Output is low
				if(VoltageLead[0] < lowerTrigger) {
					// Input voltage low enough to set output high
					state = true;
					@out = 5;
				} else {
					@out = 0;
				}
			}

			double maxStep = slewRate * simulation.TimeStep * 1e9;
			@out = Math.Max(Math.Min(v0 + maxStep, @out), v0 - maxStep);
			simulation.UpdateVoltageSource(0, LeadNode[1], VoltageSource, @out);
		}

		public override double GetVoltageDelta() {
			return VoltageLead[0];
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "InvertingSchmitt";
			arr[1] = "Vi = " + getVoltageText(LeadVoltage[0]);
			arr[2] = "Vo = " + getVoltageText(LeadVoltage[1]);
		}*/

		// There is no current path through the InvertingSchmitt input, but there
		// is an indirect path through the output to ground.
		public override bool LeadsAreConnected(int n1, int n2) {
			return false;
		}

		public override bool LeadIsGround(int n1) {
			return (n1 == 1);
		}
	}
}