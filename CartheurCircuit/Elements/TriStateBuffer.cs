namespace CartheurCircuit {

	// Contributed by Edward Calver.

	public class TriStateBuffer : CircuitElement {

		public Lead leadIn { get { return LeadZero; } }
		public Lead leadOut { get { return LeadOne; } }
		public Lead leadGate { get { return new Lead(this, 2); } }

		/// <summary>
		/// Resistance
		/// </summary>
		public double resistance { get; private set; }

		/// <summary>
		/// On Resistance (ohms)
		/// </summary>
		public double r_on { get; set; }

		/// <summary>
		/// Off Resistance (ohms)
		/// </summary>
		public double r_off { get; set; }

		/// <summary>
		/// <c>true</c> if buffer open; otherwise, <c>false</c>
		/// </summary>
		public bool open { get; private set; }

		//public ElementLead lead2;
		//public ElementLead lead3;

		public TriStateBuffer() : base() {
			//lead2 = new ElementLead(this,2);
			//lead3 = new ElementLead(this,3);
			r_on = 0.1;
			r_off = 1e10;
		}

		public override void CalculateCurrent() {
			Current = (VoltageLead[0] - VoltageLead[1]) / resistance;
		}

		// we need this to be able to change the matrix for each step
		public override bool NonLinear() { return true; }

		public override void Stamp(Circuit simulation) {
			simulation.StampVoltageSource(0, LeadNode[3], VoltageSource);
			simulation.StampNonLinear(LeadNode[3]);
			simulation.StampNonLinear(LeadNode[1]);
		}

		public override void Step(Circuit simulation) {
			open = (VoltageLead[2] < 2.5);
			resistance = (open) ? r_off : r_on;
			simulation.StampResistor(LeadNode[3], LeadNode[1], resistance);
			simulation.UpdateVoltageSource(0, LeadNode[3], VoltageSource, VoltageLead[0] > 2.5 ? 5 : 0);
		}

		public override int GetLeadCount() {
			return 4;
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "tri-state buffer";
			arr[1] = open ? "open" : "closed";
			arr[2] = "Vd = " + getVoltageDText(getVoltageDiff());
			arr[3] = "I = " + getCurrentDText(current);
			arr[4] = "Vc = " + getVoltageText(LeadVoltage[2]);
		}*/

		// we have to just assume current will flow either way, even though that
		// might cause singular matrix errors

		// 0---3----------1
		// /
		// 2

		public override bool LeadsAreConnected(int n1, int n2) {
			return (n1 == 1 && n2 == 3) || (n1 == 3 && n2 == 1);
		}
	}
}