namespace CartheurCircuit {

	public class DiodeElm : CircuitElement {

		public Lead leadIn { get { return LeadZero; } }
		public Lead leadOut { get { return LeadOne; } }

		protected Diode diode;

		/// <summary>
		/// Fwd Voltage @ 1A
		/// </summary>
		public double forwardDrop {
			get {
				return _forwardDrop;
			}
			set {
				_forwardDrop = value;
				setup();
			}
		}

		/// <summary>
		/// Zener Voltage @ 5mA
		/// </summary>
		public double zvoltage {
			get {
				return _zvoltage;
			}
			set {
				_zvoltage = value;
				setup();
			}
		}

		protected double _forwardDrop;
		protected double _zvoltage;

		protected double defaultdrop = 0.805904783;

		public DiodeElm() : base() {
			diode = new Diode();
			forwardDrop = defaultdrop;
			zvoltage = 0;
			setup();
		}

		public override bool NonLinear() { return true; }

		public virtual void setup() {
			diode.Setup(forwardDrop, zvoltage);
		}

		public override void Reset() {
			diode.Reset();
            VoltageLead[0] = VoltageLead[1] = 0;
		}

		public override void Stamp(Circuit simulation) {
			diode.Stamp(simulation, LeadNode[0], LeadNode[1]);
		}

		public override void Step(Circuit simulation) {
            diode.DoStep(simulation, VoltageLead[0] - VoltageLead[1]);
		}

		public override void CalculateCurrent() {
            Current = diode.CalculateCurrent(VoltageLead[0] - VoltageLead[1]);
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "diode";
			arr[1] = "I = " + getCurrentText(current);
			arr[2] = "Vd = " + getVoltageText(getVoltageDiff());
			arr[3] = "P = " + getUnitText(getPower(), "W");
			arr[4] = "Vf = " + getVoltageText(forwardDrop);
		}*/

	}
}