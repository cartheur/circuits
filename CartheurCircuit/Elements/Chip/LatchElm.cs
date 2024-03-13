using System;

namespace CartheurCircuit {

	public class LatchElm : Chip {

		private int loadPin;
		private bool lastLoad = false;

		public LatchElm() : base() {

		}

		public override String GetChipName() {
			return "Latch";
		}

		public override bool needsBits() {
			return true;
		}

		public override void SetupPins() {
			pins = new Pin[GetLeadCount()];
			for(int i = 0; i != bits; i++)
				pins[i] = new Pin("I" + i);
			for(int i = 0; i != bits; i++) {
				pins[i + bits] = new Pin("O" + i);
				pins[i + bits].output = true;
			}
			pins[loadPin = bits * 2] = new Pin("Ld");
			AllocateLeads();
		}

		public override void Execute(Circuit sim) {
			if(pins[loadPin].value && !lastLoad)
				for(int i = 0; i != bits; i++)
					pins[i + bits].value = pins[i].value;
			lastLoad = pins[loadPin].value;
		}

		public override int GetVoltageSourceCount() {
			return bits;
		}

		public override int GetLeadCount() {
			return bits * 2 + 1;
		}

	}
}