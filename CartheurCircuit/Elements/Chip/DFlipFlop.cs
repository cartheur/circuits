using System;

namespace CartheurCircuit {

	public class DFlipFlop : Chip {

		public Lead leadD { get { return LeadZero; } }
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
			}
		}

		public bool hasSetPin {
			get {
				return _hasSet;
			}
			set {
				_hasSet = value;
				SetupPins();
			}
		}

		private bool _hasReset;
		private bool _hasSet;

		public override String GetChipName() {
			return "D flip-flop";
		}

		public override void SetupPins() {
			pins = new Pin[GetLeadCount()];

			pins[0] = new Pin("D");

			pins[1] = new Pin(" Q");
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
				Debug.Log("flip", pins[3].value, pins[0].value);
				pins[1].value = pins[0].value;
				pins[2].value = !pins[0].value;
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