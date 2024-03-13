using System;

namespace CartheurCircuit {

	// Contributed by Edward Calver

	public class MultiplexerElm : Chip {

		public MultiplexerElm() : base() {

		}

		bool hasReset() {
			return false;
		}

		public override String GetChipName() {
			return "Multiplexer";
		}

		public override void SetupPins() {
			pins = new Pin[GetLeadCount()];

			pins[0] = new Pin("I0");
			pins[1] = new Pin("I1");
			pins[2] = new Pin("I2");
			pins[3] = new Pin("I3");

			pins[4] = new Pin("S0");
			pins[5] = new Pin("S1");

			pins[6] = new Pin("Q");
			pins[6].output = true;

		}

		public override int GetLeadCount() {
			return 7;
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

		public override void Execute(Circuit sim) {
			int selectedvalue = 0;
			if(pins[4].value)
				selectedvalue++;
			if(pins[5].value)
				selectedvalue += 2;
			pins[6].value = pins[selectedvalue].value;
		}

	}
}