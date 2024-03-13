using System;

namespace CartheurCircuit {

	public class SparkGap : CircuitElement {

		public Lead leadIn { get { return LeadZero; } }
		public Lead leadOut { get { return LeadOne; } }

		/// <summary>
		/// On resistance (ohms)
		/// </summary>
		public double onresistance { get; set; }

		/// <summary>
		/// Off resistance (ohms)
		/// </summary>
		public double offresistance { get; set; }

		/// <summary>
		/// Breakdown voltage
		/// </summary>
		public double breakdown { get; set; }

		/// <summary>
		/// Holding current (A)
		/// </summary>
		public double holdcurrent { get; set; }

		private double resistance;
		private bool state;

		public SparkGap() : base() {
			offresistance = 1E9;
			onresistance = 1E3;
			breakdown = 1E3;
			holdcurrent = 0.001;
			state = false;
		}

		public override bool NonLinear() { return true; }

		public override void CalculateCurrent() {
			double vd = VoltageLead[0] - VoltageLead[1];
			Current = vd / resistance;
		}

		public override void Reset() {
			base.Reset();
			state = false;
		}

		public override void BeginStep(Circuit simulation) {
			if(Math.Abs(Current) < holdcurrent)
				state = false;
			double vd = VoltageLead[0] - VoltageLead[1];
			if(Math.Abs(vd) > breakdown)
				state = true;
		}

		public override void Step(Circuit simulation) {
			resistance = (state) ? onresistance : offresistance;
			simulation.StampResistor(LeadNode[0], LeadNode[1], resistance);
		}

		public override void Stamp(Circuit simulation) {
			simulation.StampNonLinear(LeadNode[0]);
			simulation.StampNonLinear(LeadNode[1]);
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "spark gap";
			getBasicInfo(arr);
			arr[3] = state ? "on" : "off";
			arr[4] = "Ron = " + getUnitText(onresistance, Circuit.ohmString);
			arr[5] = "Roff = " + getUnitText(offresistance, Circuit.ohmString);
			arr[6] = "Vbreakdown = " + getUnitText(breakdown, "V");
		}*/

	}
}