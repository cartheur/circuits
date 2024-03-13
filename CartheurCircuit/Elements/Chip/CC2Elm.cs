using System;

namespace CartheurCircuit {

	public class CC2Elm : Chip {

		private double gain;

		public CC2Elm() : base() {
			gain = 1;
		}

		public CC2Elm(int g) : base() {
			gain = g;
		}

		public override String GetChipName() {
			return "CC2";
		}

		public override void SetupPins() {
			pins = new Pin[3];
			pins[0] = new Pin("X");
			pins[0].output = true;
			pins[1] = new Pin("Y");
			pins[2] = new Pin("Z");
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = (gain == 1) ? "CCII+" : "CCII-";
			arr[1] = "X,Y = " + getVoltageText(LeadVoltage[0]);
			arr[2] = "Z = " + getVoltageText(LeadVoltage[2]);
			arr[3] = "I = " + getCurrentText(pins[0].current);
		}*/

		// boolean nonLinear() { return true; }
		public override void Stamp(Circuit simulation) {
			// X voltage = Y voltage
			simulation.StampVoltageSource(0, LeadNode[0], pins[0].VoltageSource);
			simulation.StampVcvs(0, LeadNode[1], 1, pins[0].VoltageSource);
			// Z current = gain * X current
			simulation.StampCccs(0, LeadNode[2], pins[0].VoltageSource, gain);
		}

		public override int GetLeadCount() {
			return 3;
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

	}

	class CC2NegElm : CC2Elm {

		public CC2NegElm() : base() {

		}

	}
}