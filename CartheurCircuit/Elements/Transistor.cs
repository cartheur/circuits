using System;

namespace CartheurCircuit {

	public class Transistor : CircuitElement {

		private static readonly double leakage = 1E-13; // 1e-6;
		private static readonly double vt = 0.025;
		private static readonly double vdcoef = 1 / vt;
		private static readonly double rgain = 0.5;

		public Lead leadBase { get { return LeadZero; } }
		public Lead leadCollector { get { return LeadOne; } }
		public Lead leadEmitter { get { return new Lead(this, 2); } }

		/// <summary>
		/// Beta/hFE
		/// </summary>
		public double Beta {
			get {
				return beta;
			}
			set {
				beta = value;
				setup();
			}
		}

		public bool IsPNP {
			get {
				return pnp == 1;
			}
			set {
				pnp = (value) ? -1 : 1;
			}
		}

		private double beta;

		private double pnp;

		private double fgain;
		private double gmin;

		private double ic, ie, ib;
		private double vcrit;
		private double lastvbc, lastvbe;

		public Transistor() : base() {
			pnp = 1;
			beta = 100;
			setup();
		}

		public Transistor(bool pnpflag) : base() {
			pnp = (pnpflag) ? -1 : 1;
			beta = 100;
			setup();
		}

		public void setup() {
			vcrit = vt * Math.Log(vt / (Math.Sqrt(2) * leakage));
			fgain = beta / (beta + 1);
		}

		public override bool NonLinear() { return true; }

		public override void Reset() {
			VoltageLead[0] = VoltageLead[1] = VoltageLead[2] = 0;
			lastvbc = lastvbe = 0;
		}

		public override int GetLeadCount() {
			return 3;
		}

		/*public override double getPower() {
			return (LeadVoltage[0] - LeadVoltage[2]) * ib + (LeadVoltage[1] - LeadVoltage[2]) * ic;
		}*/

		public double limitStep(Circuit sim, double vnew, double vold) {
			double arg;
			if(vnew > vcrit && Math.Abs(vnew - vold) > (vt + vt)) {
				if(vold > 0) {
					arg = 1 + (vnew - vold) / vt;
					if(arg > 0) {
						vnew = vold + vt * Math.Log(arg);
					} else {
						vnew = vcrit;
					}
				} else {
					vnew = vt * Math.Log(vnew / vt);
				}
				sim.Converged = false;
				// System.out.println(vnew + " " + oo + " " + vold);
			}
			return (vnew);
		}

		public override void Stamp(Circuit simulation) {
			simulation.StampNonLinear(LeadNode[0]);
			simulation.StampNonLinear(LeadNode[1]);
			simulation.StampNonLinear(LeadNode[2]);
		}

		public override void Step(Circuit simulation) {
			double vbc = VoltageLead[0] - VoltageLead[1]; // typically negative
			double vbe = VoltageLead[0] - VoltageLead[2]; // typically positive
			if(Math.Abs(vbc - lastvbc) > 0.01 || Math.Abs(vbe - lastvbe) > 0.01)
				simulation.Converged = false;
			
			gmin = 0;
			if(simulation.SubIterations > 100) {
				// if we have trouble converging, put a conductance in parallel with
				// all P-N junctions. Gradually increase the conductance value for each iteration.
				gmin = Math.Exp(-9 * Math.Log(10) * (1 - simulation.SubIterations / 3000.0));
				if(gmin > .1) gmin = .1;
			}
			
			vbc = pnp * limitStep(simulation, pnp * vbc, pnp * lastvbc);
			vbe = pnp * limitStep(simulation, pnp * vbe, pnp * lastvbe);
			lastvbc = vbc;
			lastvbe = vbe;
			double pcoef = vdcoef * pnp;
			double expbc = Math.Exp(vbc * pcoef);
			
			double expbe = Math.Exp(vbe * pcoef);
			if(expbe < 1) expbe = 1;
			
			ie = pnp * leakage * (-(expbe - 1) + rgain * (expbc - 1));
			ic = pnp * leakage * (fgain * (expbe - 1) - (expbc - 1));
			ib = -(ie + ic);
			
			double gee = -leakage * vdcoef * expbe;
			double gec = rgain * leakage * vdcoef * expbc;
			double gce = -gee * fgain;
			double gcc = -gec * (1 / rgain);

			// stamps from page 302 of Pillage. Node 0 is the base,
			// node 1 the collector, node 2 the emitter. Also stamp
			// minimum conductance (gmin) between b,e and b,c
			simulation.StampMatrix(LeadNode[0], LeadNode[0], -gee - gec - gce - gcc + gmin * 2);
			simulation.StampMatrix(LeadNode[0], LeadNode[1], gec + gcc - gmin);
			simulation.StampMatrix(LeadNode[0], LeadNode[2], gee + gce - gmin);
			simulation.StampMatrix(LeadNode[1], LeadNode[0], gce + gcc - gmin);
			simulation.StampMatrix(LeadNode[1], LeadNode[1], -gcc + gmin);
			simulation.StampMatrix(LeadNode[1], LeadNode[2], -gce);
			simulation.StampMatrix(LeadNode[2], LeadNode[0], gee + gec - gmin);
			simulation.StampMatrix(LeadNode[2], LeadNode[1], -gec);
			simulation.StampMatrix(LeadNode[2], LeadNode[2], -gee + gmin);

			// we are solving for v(k+1), not delta v, so we use formula
			// 10.5.13, multiplying J by v(k)
			simulation.StampRightSide(LeadNode[0], -ib - (gec + gcc) * vbc - (gee + gce) * vbe);
			simulation.StampRightSide(LeadNode[1], -ic + gce * vbe + gcc * vbc);
			simulation.StampRightSide(LeadNode[2], -ie + gee * vbe + gec * vbc);
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = "transistor (" + ((pnp == -1) ? "PNP)" : "NPN)") + " beta=" + beta;
			double vbc = LeadVoltage[0] - LeadVoltage[1];
			double vbe = LeadVoltage[0] - LeadVoltage[2];
			double vce = LeadVoltage[1] - LeadVoltage[2];
			if(vbc * pnp > .2) {
				arr[1] = vbe * pnp > .2 ? "saturation" : "reverse active";
			} else {
				arr[1] = vbe * pnp > .2 ? "fwd active" : "cutoff";
			}
			arr[2] = "Ic = " + getCurrentText(ic);
			arr[3] = "Ib = " + getCurrentText(ib);
			arr[4] = "Vbe = " + getVoltageText(vbe);
			arr[5] = "Vbc = " + getVoltageText(vbc);
			arr[6] = "Vce = " + getVoltageText(vce);
		}*/

		/*public override double getScopeValue(int x) {
			switch (x) {
			case 1:
				return ib;
			case 2:
				return ic;
			case 3:
				return ie;
			case 4:
				return volts[0] - volts[2];
			case 5:
				return volts[0] - volts[1];
			case 6:
				return volts[1] - volts[2];
			}
			return 0;
		}*/

		/*public override String getScopeUnits(int x) {
			switch (x) {
			case 1:
			case 2:
			case 3:
				return "A";
			default:
				return "V";
			}
		}*/

		/*public override bool canViewInScope() {
			return true;
		}*/
	}
}