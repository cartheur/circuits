using System;

namespace CartheurCircuit {

	public class TFlipFlopElm : Chip {

		public Lead leadT { get { return LeadZero; } }
		public Lead leadQ { get { return LeadOne; } }
		public Lead leadQL { get { return new Lead(this, 2); } }
		public Lead leadCLK { get { return new Lead(this, 3); } }

		public bool hasResetPin {
			get {
				return _hasReset;
			}
			set {
				_hasReset = value;
				SetupPins();
				AllocateLeads();
			}
		}

		public bool hasSetPin {
			get {
				return _hasSet;
			}
			set {
				_hasSet = value;
				SetupPins();
				AllocateLeads();
			}
		}

		private bool _hasReset;
		private bool _hasSet;

		private bool last_val;

		public override String GetChipName() {
			return "T flip-flop";
		}

		public override void SetupPins() {
			pins = new Pin[GetLeadCount()];

			pins[0] = new Pin("T");

			pins[1] = new Pin("Q");
			pins[1].output = true;

			pins[2] = new Pin("|Q");
			pins[2].output = true;
			pins[2].lineOver = true;

			pins[3] = new Pin("");
			pins[3].clock = true;

			if(!hasSetPin) {
				if(hasResetPin)
					pins[4] = new Pin("R");
			} else {
				pins[5] = new Pin("S");
				pins[4] = new Pin("R");
			}
		}

		public override int GetLeadCount() {
			return 4 + (hasResetPin ? 1 : 0) + (hasSetPin ? 1 : 0);
		}

		public override int GetVoltageSourceCount() {
			return 2;
		}

		public override void Reset() {
			base.Reset();
			VoltageLead[2] = 5;
			pins[2].value = true;
		}

		public override void Execute(Circuit sim) {
			if(pins[3].value && !lastClock) {
				if(pins[0].value) { // if T = 1 {
					pins[1].value = !last_val;
					pins[2].value = !pins[1].value;
					last_val = !last_val;
				}
				// else no change

			}
			if(hasSetPin && pins[5].value) {
				pins[1].value = true;
				pins[2].value = false;
			}
			if(hasResetPin && pins[4].value) {
				pins[1].value = false;
				pins[2].value = true;
			}
			lastClock = pins[3].value;
		}

	}
}