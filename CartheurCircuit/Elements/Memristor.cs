namespace CartheurCircuit {
	
	public class Memristor : CircuitElement {

		public Lead LeadIn { get { return LeadZero; } }
		public Lead LeadOut { get { return LeadOne; } }

		/// <summary>
		/// Max Resistance (ohms)
		/// </summary>
		public double r_on { get; set; }

		/// <summary>
		/// Min Resistance (ohms)
		/// </summary>
		public double r_off { get; set; }

		/// <summary>
		/// Width of Doped Region (nm)
		/// </summary>
		public double dopeWidth {
			get {
				return _dopeWidth * 1E9;
			}
			set {
				_dopeWidth = value * 1E-9;
			}
		}

		/// <summary>
		/// Total Width (nm)
		/// </summary>
		public double totalWidth {
			get {
				return _totalWidth * 1E9;
			}
			set {
				_totalWidth = value * 1E-9;
			}
		}

		/// <summary>
		/// Mobility (um^2/(s*V))
		/// </summary>
		public double mobility {
			get {
				return _mobility * 1E12;
			}
			set {
				_mobility = value * 1E12;
			}
		}

		private double _dopeWidth;
		private double _totalWidth;
		private double _mobility;

		private double resistance;

		public Memristor() : base() {
			r_on = 100;
			r_off = 160 * r_on;
			_dopeWidth = 0;
			_totalWidth = 10E-9; // meters
			_mobility = 1E-10; // m^2/sV
			resistance = 100;
		}

		public override bool NonLinear() { return true; }

		public override void CalculateCurrent() {
			Current = (VoltageLead[0] - VoltageLead[1]) / resistance;
		}

		public override void Reset() {
			_dopeWidth = 0;
		}

		public override void BeginStep(Circuit simulation) {
			double wd = _dopeWidth / _totalWidth;
			_dopeWidth += simulation.TimeStep * _mobility * r_on * Current / _totalWidth;
			if (_dopeWidth < 0)
				_dopeWidth = 0;
			if (_dopeWidth > _totalWidth)
				_dopeWidth = _totalWidth;
			resistance = r_on * wd + r_off * (1 - wd);
		}

		public override void Stamp(Circuit simulation) {
			simulation.StampNonLinear(LeadNode[0]);
			simulation.StampNonLinear(LeadNode[1]);
		}

		public override void Step(Circuit simulation) {
			simulation.StampResistor(LeadNode[0], LeadNode[1], resistance);
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "memristor";
			getBasicInfo(arr);
			arr[3] = "R = " + getUnitText(resistance, Circuit.ohmString);
			arr[4] = "P = " + getUnitText(getPower(), "W");
		}*/

		//public override double getScopeValue(int x) {
		//	return (x == 2) ? resistance : (x == 1) ? getPower() : getVoltageDiff();
		//}

		//public override String getScopeUnits(int x) {
		//	return (x == 2) ? CirSim.ohmString : (x == 1) ? "W" : "V";
		//}

	}
}