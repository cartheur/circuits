using System;

namespace CartheurCircuit {

	public class DecadeElm : Chip {

		public DecadeElm() : base() {

		}

		public override String GetChipName() {
			return "decade counter";
		}

		public override bool needsBits() {
			return true;
		}

		public override void SetupPins() {
			pins = new Pin[GetLeadCount()];
			pins[0] = new Pin("");
			pins[0].clock = true;
			pins[1] = new Pin("R");
			for(int i = 0; i != bits; i++) {
				int ii = i + 2;
				pins[ii] = new Pin("Q" + i);
				pins[ii].output = true;
			}
			AllocateLeads();
		}

		public override int GetLeadCount() {
			return bits + 2;
		}

		public override int GetVoltageSourceCount() {
			return bits;
		}

		public override void Execute(Circuit sim) {
			int i;
			if(pins[0].value && !lastClock) {
				for(i = 0; i != bits; i++) {
					if(pins[i + 2].value)
						break;
				}
				if(i < bits)
					pins[i++ + 2].value = false;
				i %= bits;
				pins[i + 2].value = true;
			}
			if(!pins[1].value) {
				for(i = 1; i != bits; i++)
					pins[i + 2].value = false;
				pins[2].value = true;
			}
			lastClock = pins[0].value;
		}

	}
}