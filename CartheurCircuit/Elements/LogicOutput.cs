namespace CartheurCircuit {

	public class LogicOutput : Output {

		/// <summary>
		/// The Threshold Voltage.
		/// </summary>
		public double threshold { get; set; }

		public bool needsPullDown { get; set; }

		public LogicOutput() : base() {
			threshold = 2.5;
		}

		public override void Stamp(Circuit simulation) {
			if(needsPullDown)
				simulation.StampResistor(LeadNode[0], 0, 1E6);
		}

		public bool isHigh() {
			return (VoltageLead[0] < threshold) ? false : true;
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "logic output";
			arr[1] = (LeadVoltage[0] < threshold) ? "low" : "high";
			arr[2] = "V = " + getVoltageText(LeadVoltage[0]);
		}*/

	}
}