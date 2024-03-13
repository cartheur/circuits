using System;

namespace CartheurCircuit {

	public class FullAdder : Chip {

		public Lead leadOut1 { get { return LeadZero; } }
		public Lead leadOut2 { get { return LeadOne; } }
		public Lead leadIn0 { get { return new Lead(this, 2); } }
		public Lead leadIn1 { get { return new Lead(this, 3); } }
		public Lead leadIn2 { get { return new Lead(this, 4); } }
		
		public override String GetChipName() {
			return "Full Adder";
		}

		bool hasReset() {
			return false;
		}

		public override void SetupPins() {
			pins = new Pin[GetLeadCount()];
			pins[0] = new Pin("Out1"); // S
			pins[0].output = true;
			pins[1] = new Pin("Out2"); // C
			pins[1].output = true;
			pins[2] = new Pin("In0");
			pins[3] = new Pin("In1");
			pins[4] = new Pin("In2");

		}

		public override int GetLeadCount() {
			return 5;
		}

		public override int GetVoltageSourceCount() {
			return 2;
		}

		public override void Execute(Circuit sim) {
			pins[0].value = (pins[2].value ^ pins[3].value) ^ pins[4].value;
			pins[1].value = (pins[2].value && pins[3].value) || (pins[2].value && pins[4].value) || (pins[3].value && pins[4].value);
		}

	}
}