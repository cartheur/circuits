using System;

namespace CartheurCircuit.Elements.Sources {

	// Contributed by Edward Calver.

	public class AmSource : CircuitElement {

		public Lead leadOut { get { return LeadZero; } }

		/// <summary>
		/// Carrier Frequency (Hz)
		/// </summary>
		public double carrierfreq { get; set; }

		/// <summary>
		/// Signal Frequency (Hz)
		/// </summary>
		public double signalfreq { get; set; }

		/// <summary>
		/// Max Voltage
		/// </summary>
		public double maxVoltage { get; set; }

		private double freqTimeZero;

		public AmSource() : base() {
			maxVoltage = 5;
			carrierfreq = 1000;
			signalfreq = 40;
			Reset();
		}

		public override void Reset() {
			freqTimeZero = 0;
		}

		public override int GetLeadCount() {
			return 1;
		}

		public override void Stamp(Circuit simulation) {
			simulation.StampVoltageSource(0, LeadNode[0], VoltageSource);
		}

		public override void Step(Circuit simulation) {
			simulation.UpdateVoltageSource(0, LeadNode[0], VoltageSource, getVoltage(simulation.Time));
		}

		public double getVoltage(double time) {
			double w = 2 * Pi * (time - freqTimeZero);
			return ((Math.Sin(w * signalfreq) + 1) / 2) * Math.Sin(w * carrierfreq) * maxVoltage;
		}

		public override double GetVoltageDelta() {
			return VoltageLead[0];
		}

		public override bool LeadIsGround(int n1) {
			return true;
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

		/*public override double getPower() {
			return -getVoltageDiff() * current;
		}*/

		/*public override void getInfo(String[] arr) {
			arr[0] = "AM Source";
			arr[1] = "I = " + getCurrentText(current);
			arr[2] = "V = " + getVoltageText(getVoltageDiff());
			arr[3] = "cf = " + getUnitText(carrierfreq, "Hz");
			arr[4] = "sf = " + getUnitText(signalfreq, "Hz");
			arr[5] = "Vmax = " + getVoltageText(maxVoltage);
		}*/
	}
}