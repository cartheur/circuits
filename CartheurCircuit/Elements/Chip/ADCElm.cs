using System;

namespace CartheurCircuit {

	public class ADCElm : Chip {
		
		public ADCElm() : base() {
			
		}

		public override String GetChipName() {
			return "ADC";
		}

		public override bool needsBits() {
			return true;
		}

		public override void SetupPins() {
			pins = new Pin[GetLeadCount()];
			for (int i = 0; i != bits; i++) {
				pins[i] = new Pin("D"+i);
				pins[i].output = true;
			}
			pins[bits] = new Pin("In");
			pins[bits + 1] = new Pin("V+");
			AllocateLeads();
		}

		public override void Execute(Circuit sim) {
			int imax = (1 << bits) - 1;
			// if we round, the half-flash doesn't work
			double val = imax * VoltageLead[bits] / VoltageLead[bits + 1]; // + .5;
			int ival = (int) val;
			ival = Math.Min(imax, Math.Max(0, ival));
			for (int i = 0; i != bits; i++)
				pins[i].value = ((ival & (1 << i)) != 0);
		}

		public override int GetVoltageSourceCount() {
			return bits;
		}

		public override int GetLeadCount() {
			return bits + 2;
		}

	}
}