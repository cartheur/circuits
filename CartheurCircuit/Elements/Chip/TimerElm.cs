using System;

namespace CartheurCircuit {

	public class TimerElm : Chip {

		public bool hasResetPin {
			get {
				return _hasReset;
			}
			set {
				_hasReset = value;
				SetupPins();
				AllocateLeads();
			}
		}

		public int N_DIS = 0;
		public int N_TRIG = 1;
		public int N_THRES = 2;
		public int N_VIN = 3;
		public int N_CTL = 4;
		public int N_OUT = 5;
		public int N_RST = 6;

		private bool _hasReset = true;

		private bool setOut, @out;

		public TimerElm() : base() {

		}

		public override String GetChipName() {
			return "555 Timer";
		}

		public override void SetupPins() {
			pins = new Pin[7];
			pins[N_DIS] = new Pin("dis");
			pins[N_TRIG] = new Pin("tr");
			pins[N_TRIG].lineOver = true;
			pins[N_THRES] = new Pin("th");
			pins[N_VIN] = new Pin("Vin");
			pins[N_CTL] = new Pin("ctl");
			pins[N_OUT] = new Pin("out");
			pins[N_OUT].output = true;
			pins[N_RST] = new Pin("rst");
		}

		public override bool NonLinear() { return true; }

		public override void Stamp(Circuit simulation) {
			// stamp voltage divider to put ctl pin at 2/3 V
			simulation.StampResistor(LeadNode[N_VIN], LeadNode[N_CTL], 5000);
			simulation.StampResistor(LeadNode[N_CTL], 0, 10000);
			// output pin
			simulation.StampVoltageSource(0, LeadNode[N_OUT], pins[N_OUT].VoltageSource);
			// discharge pin
			simulation.StampNonLinear(LeadNode[N_DIS]);
		}

		public override void CalculateCurrent() {
			// need current for V, discharge, control; output current is
			// calculated for us, and other pins have no current
			pins[N_VIN].current = (VoltageLead[N_CTL] - VoltageLead[N_VIN]) / 5000;
			pins[N_CTL].current = -VoltageLead[N_CTL] / 10000 - pins[N_VIN].current;
			pins[N_DIS].current = (!@out && !setOut) ? -VoltageLead[N_DIS] / 10 : 0;
		}

		public override void BeginStep(Circuit simulation) {
			@out = VoltageLead[N_OUT] > VoltageLead[N_VIN] / 2;
			setOut = false;
			// check comparators
			if(VoltageLead[N_CTL] / 2 > VoltageLead[N_TRIG])
				setOut = @out = true;
			if(VoltageLead[N_THRES] > VoltageLead[N_CTL] || (hasResetPin && VoltageLead[N_RST] < .7))
				@out = false;
		}

		public override void Step(Circuit simulation) {
			// if output is low, discharge pin 0. we use a small
			// resistor because it's easier, and sometimes people tie
			// the discharge pin to the trigger and threshold pins.
			// We check setOut to properly emulate the case where
			// trigger is low and threshold is high.
			if(!@out && !setOut)
				simulation.StampResistor(LeadNode[N_DIS], 0, 10);
			// output
			simulation.UpdateVoltageSource(0, LeadNode[N_OUT], pins[N_OUT].VoltageSource, @out ? VoltageLead[N_VIN] : 0);
		}

		public override int GetLeadCount() {
			return hasResetPin ? 7 : 6;
		}

		public override int GetVoltageSourceCount() {
			return 1;
		}

	}
}