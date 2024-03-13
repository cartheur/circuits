using System;

namespace CartheurCircuit {

	public class CounterElm : Chip {

		public bool hasEnable {
			get {
				return _hasEnable;
			}
			set {
				_hasEnable = value;
				SetupPins();
			}
		}

		public bool invertReset { get; set; }

		private bool _hasEnable;

		public override bool needsBits() {
			return true;
		}

		public override String GetChipName() {
			return "Counter";
		}

		public override void SetupPins() {
			pins = new Pin[GetLeadCount()];
			pins[0] = new Pin("");
			pins[0].clock = true;
			pins[1] = new Pin("R");
			for(int i = 0; i != bits; i++) {
				int ii = i + 2;
				pins[ii] = new Pin("Q" + (bits - i - 1));
				pins[ii].output = true;
			}
			if(hasEnable)
				pins[bits + 2] = new Pin("En");
			AllocateLeads();
		}

		public override int GetLeadCount() {
			if(hasEnable)
				return bits + 3;
			return bits + 2;
		}

		public override int GetVoltageSourceCount() {
			return bits;
		}

		public override void Execute(Circuit sim) {
			bool en = true;
			if(hasEnable)
				en = pins[bits + 2].value;
			if(pins[0].value && !lastClock && en) {
				for(int i = bits - 1; i >= 0; i--) {
					int ii = i + 2;
					if(!pins[ii].value) {
						pins[ii].value = true;
						break;
					}
					pins[ii].value = false;
				}
			}
			if(!pins[1].value == invertReset) {
				for(int i = 0; i != bits; i++)
					pins[i + 2].value = false;
			}
			lastClock = pins[0].value;
		}

	}
}