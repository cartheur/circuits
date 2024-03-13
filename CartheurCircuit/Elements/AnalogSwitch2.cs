namespace CartheurCircuit {

	// Unfinished

	public class AnalogSwitch2 : AnalogSwitch {

		public override int GetLeadCount() {
			return 4;
		}

		public override void CalculateCurrent() {
			if(IsOpen) {
				Current = (VoltageLead[0] - VoltageLead[2]) / OnResistance;
			} else {
				Current = (VoltageLead[0] - VoltageLead[1]) / OnResistance;
			}
		}

		public override void Stamp(Circuit simulation) {
			simulation.StampNonLinear(LeadNode[0]);
			simulation.StampNonLinear(LeadNode[1]);
			simulation.StampNonLinear(LeadNode[2]);
		}

		public override void Step(Circuit simulation) {
			IsOpen = (VoltageLead[3] < 2.5);
			if(invert) IsOpen = !IsOpen;
			if(IsOpen) {
				simulation.StampResistor(LeadNode[0], LeadNode[2], OnResistance);
				simulation.StampResistor(LeadNode[0], LeadNode[1], OffResistance);
			} else {
				simulation.StampResistor(LeadNode[0], LeadNode[1], OnResistance);
				simulation.StampResistor(LeadNode[0], LeadNode[2], OffResistance);
			}
		}

		public override bool LeadsAreConnected(int n1, int n2) {
			return !(n1 == 3 || n2 == 3);
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "analog switch (SPDT)";
			arr[1] = "I = " + getCurrentDText(current);
		}*/
	}
}