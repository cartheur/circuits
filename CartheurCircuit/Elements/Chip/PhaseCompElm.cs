using System;

namespace CartheurCircuit {

	public class PhaseCompElm : Chip {

		private bool ff1, ff2;

		public PhaseCompElm() : base() {

		}

		public override String GetChipName() {
			return "phase comparator";
		}

		public override void SetupPins() {
			pins = new Pin[3];
			pins[0] = new Pin("I1");
			pins[1] = new Pin("I2");
			pins[2] = new Pin("O");
			pins[2].output = true;
		}

		public override bool NonLinear() { return true; }

		public override void Stamp(Circuit simulation) {
			int vn = simulation.NodeCount + pins[2].VoltageSource;
			simulation.StampNonLinear(vn);
			simulation.StampNonLinear(0);
			simulation.StampNonLinear(LeadNode[2]);
		}

		public override void Step(Circuit simulation) {
			bool v1 = VoltageLead[0] > 2.5;
			bool v2 = VoltageLead[1] > 2.5;
			if(v1 && !pins[0].value)
				ff1 = true;
			if(v2 && !pins[1].value)
				ff2 = true;
			if(ff1 && ff2)
				ff1 = ff2 = false;
			double @out = (ff1) ? 5 : (ff2) ? 0 : -1;
			// System.out.println(out + " " + v1 + " " + v2);
			if(@out != -1) {
				simulation.StampVoltageSource(0, LeadNode[2], pins[2].VoltageSource, @out);
			} else {
				// tie current through output pin to 0
				int vn = simulation.NodeCount + pins[2].VoltageSource;
				simulation.StampMatrix(vn, vn, 1);
			}
			pins[0].value = v1;
			pins[1].value = v2;
		}

		public override int GetLeadCount() {
			return 3;
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

	}
}