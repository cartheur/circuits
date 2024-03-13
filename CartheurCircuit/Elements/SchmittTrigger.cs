using System;

namespace CartheurCircuit {

	// Contributed by Edward Calver.

	public class SchmittTrigger : InvertingSchmittTrigger {

		public override void Step(Circuit simulation) {
			double v0 = VoltageLead[1];
			double @out;
			if(state) {
				// Output is high
				if(VoltageLead[0] > upperTrigger) {
					// Input voltage high enough to set output high
					state = false;
					@out = 5;
				} else {
					@out = 0;
				}
			} else {
				// Output is low
				if(VoltageLead[0] < lowerTrigger) {
					// Input voltage low enough to set output low
					state = true;
					@out = 0;
				} else {
					@out = 5;
				}
			}
			double maxStep = slewRate * simulation.TimeStep * 1e9;
			@out = Math.Max(Math.Min(v0 + maxStep, @out), v0 - maxStep);
			simulation.UpdateVoltageSource(0, LeadNode[1], VoltageSource, @out);
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "Schmitt";
		}*/

	}
}