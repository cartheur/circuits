using System;

namespace CartheurCircuit {

	public class VCOElm : Chip {
		
		public VCOElm() : base() {
			
		}

		public override String GetChipName() {
			return "VCO";
		}

		public override void SetupPins() {
			pins = new Pin[6];
			pins[0] = new Pin("Vi");
			pins[1] = new Pin("Vo");
			pins[1].output = true;
			pins[2] = new Pin("C");
			pins[3] = new Pin("C");
			pins[4] = new Pin("R1");
			pins[4].output = true;
			pins[5] = new Pin("R2");
			pins[5].output = true;
		}

		public override bool NonLinear() { return true; }

		public override void Stamp(Circuit simulation) {
			// output pin
			simulation.StampVoltageSource(0, LeadNode[1], pins[1].VoltageSource);
			// attach Vi to R1 pin so its current is proportional to Vi
			simulation.StampVoltageSource(LeadNode[0], LeadNode[4], pins[4].VoltageSource, 0);
			// attach 5V to R2 pin so we get a current going
			simulation.StampVoltageSource(0, LeadNode[5], pins[5].VoltageSource, 5);
			// put resistor across cap pins to give current somewhere to go
			// in case cap is not connected
			simulation.StampResistor(LeadNode[2], LeadNode[3], cResistance);
			simulation.StampNonLinear(LeadNode[2]);
			simulation.StampNonLinear(LeadNode[3]);
		}

		public double cResistance = 1e6;
		public double cCurrent;
		public int cDir;

		public override void Step(Circuit simulation) {
			double vc = VoltageLead[3] - VoltageLead[2];
			double vo = VoltageLead[1];
			int dir = (vo < 2.5) ? 1 : -1;
			// switch direction of current through cap as we oscillate
			if (vo < 2.5 && vc > 4.5) {
				vo = 5;
				dir = -1;
			}
			if (vo > 2.5 && vc < .5) {
				vo = 0;
				dir = 1;
			}

			// generate output voltage
			simulation.UpdateVoltageSource(0, LeadNode[1], pins[1].VoltageSource, vo);
			// now we set the current through the cap to be equal to the
			// current through R1 and R2, so we can measure the voltage
			// across the cap
			int cur1 = simulation.NodeCount + pins[4].VoltageSource;
			int cur2 = simulation.NodeCount + pins[5].VoltageSource;
			simulation.StampMatrix(LeadNode[2], cur1, dir);
			simulation.StampMatrix(LeadNode[2], cur2, dir);
			simulation.StampMatrix(LeadNode[3], cur1, -dir);
			simulation.StampMatrix(LeadNode[3], cur2, -dir);
			cDir = dir;
		}

		// can't do this in calculateCurrent() because it's called before
		// we get pins[4].current and pins[5].current, which we need
		public void computeCurrent() {
			if (cResistance == 0) {
				return;
			}
			double c = cDir * (pins[4].current + pins[5].current)
					+ (VoltageLead[3] - VoltageLead[2]) / cResistance;
			pins[2].current = -c;
			pins[3].current = c;
			pins[0].current = -pins[4].current;
		}

		public override int GetLeadCount() {
			return 6;
		}

		public override int GetVoltageSourceCount() {
			return 3;
		}

	}
}