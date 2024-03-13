using System;

namespace CartheurCircuit {

	public class DACElm : Chip {

		public DACElm() : base() {

		}

		public override String GetChipName() {
			return "DAC";
		}

		public override bool needsBits() {
			return true;
		}

		public override void SetupPins() {
			pins = new Pin[GetLeadCount()];
			for(int i = 0; i != bits; i++)
				pins[i] = new Pin("D" + i);
			pins[bits] = new Pin("O");
			pins[bits].output = true;
			pins[bits + 1] = new Pin("V+");
			AllocateLeads();
		}

		public override void Step(Circuit simulation) {
			int ival = 0;
			for(int i = 0; i != bits; i++)
				if(VoltageLead[i] > 2.5)
					ival |= 1 << i;
			int ivalmax = (1 << bits) - 1;
			double v = ival * VoltageLead[bits + 1] / ivalmax;
			simulation.UpdateVoltageSource(0, LeadNode[bits], pins[bits].VoltageSource, v);
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

		public override int GetLeadCount() {
			return bits + 2;
		}

	}
}