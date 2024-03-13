using System;

namespace CartheurCircuit {

	public class Triode : CircuitElement {

		public Lead leadPlate { get { return LeadZero; } }
		public Lead leadGrid { get { return LeadOne; } }
		public Lead leadCath { get { return new Lead(this, 2); } }

		public double currentp, currentg, currentc;

		private double mu, kg1;
		private double gridCurrentR = 6000;
		private double lastv0, lastv1, lastv2;

		public Triode() : base() {
			mu = 93;
			kg1 = 680;
		}

		public override bool NonLinear() { return true; }

		public override void Reset() {
			VoltageLead[0] = VoltageLead[1] = VoltageLead[2] = 0;
		}

		public override int GetLeadCount() {
			return 3;
		}

		/*public override double getPower() {
			return (LeadVoltage[0] - LeadVoltage[2]) * current;
		}*/

		public override void Step(Circuit simulation) {
			double[] vs = new double[3];
			vs[0] = VoltageLead[0];
			vs[1] = VoltageLead[1];
			vs[2] = VoltageLead[2];
			if(vs[1] > lastv1 + 0.5) vs[1] = lastv1 + 0.5;
			if(vs[1] < lastv1 - 0.5) vs[1] = lastv1 - 0.5;
			if(vs[2] > lastv2 + 0.5) vs[2] = lastv2 + 0.5;
			if(vs[2] < lastv2 - 0.5) vs[2] = lastv2 - 0.5;
			int grid = 1;
			int cath = 2;
			int plate = 0;
			double vgk = vs[grid] - vs[cath];
			double vpk = vs[plate] - vs[cath];
			if(Math.Abs(lastv0 - vs[0]) > 0.01 || Math.Abs(lastv1 - vs[1]) > 0.01 || Math.Abs(lastv2 - vs[2]) > 0.01)
				simulation.Converged = false;
			lastv0 = vs[0];
			lastv1 = vs[1];
			lastv2 = vs[2];
			double ids = 0;
			double gm = 0;
			double Gds = 0;
			double ival = vgk + vpk / mu;
			currentg = 0;
			if(vgk > .01) {
				simulation.StampResistor(LeadNode[grid], LeadNode[cath], gridCurrentR);
				currentg = vgk / gridCurrentR;
			}
			if(ival < 0) {
				// should be all zero, but that causes a singular matrix,
				// so instead we treat it as a large resistor
				Gds = 1E-8;
				ids = vpk * Gds;
			} else {
				ids = Math.Pow(ival, 1.5) / kg1;
				double q = 1.5 * Math.Sqrt(ival) / kg1;
				// gm = dids/dgk;
				// Gds = dids/dpk;
				Gds = q;
				gm = q / mu;
			}
			currentp = ids;
			currentc = ids + currentg;
			double rs = -ids + Gds * vpk + gm * vgk;
			simulation.StampMatrix(LeadNode[plate], LeadNode[plate], Gds);
			simulation.StampMatrix(LeadNode[plate], LeadNode[cath], -Gds - gm);
			simulation.StampMatrix(LeadNode[plate], LeadNode[grid], gm);

			simulation.StampMatrix(LeadNode[cath], LeadNode[plate], -Gds);
			simulation.StampMatrix(LeadNode[cath], LeadNode[cath], Gds + gm);
			simulation.StampMatrix(LeadNode[cath], LeadNode[grid], -gm);

			simulation.StampRightSide(LeadNode[plate], rs);
			simulation.StampRightSide(LeadNode[cath], -rs);
		}

		public override void Stamp(Circuit simulation) {
			simulation.StampNonLinear(LeadNode[0]);
			simulation.StampNonLinear(LeadNode[1]);
			simulation.StampNonLinear(LeadNode[2]);
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "triode";
			double vbc = LeadVoltage[0] - LeadVoltage[1];
			double vbe = LeadVoltage[0] - LeadVoltage[2];
			double vce = LeadVoltage[1] - LeadVoltage[2];
			arr[1] = "Vbe = " + getVoltageText(vbe);
			arr[2] = "Vbc = " + getVoltageText(vbc);
			arr[3] = "Vce = " + getVoltageText(vce);
		}*/

		// grid not connected to other terminals
		public override bool LeadsAreConnected(int n1, int n2) {
			return !(n1 == 1 || n2 == 1);
		}
	}
}