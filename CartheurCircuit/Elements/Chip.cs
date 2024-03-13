using System;

namespace CartheurCircuit {

	public abstract class Chip : CircuitElement {

		protected int bits;
		protected Pin[] pins;
		protected bool lastClock;

		public Chip() : base() {
			if(needsBits())
				bits = (this is DecadeElm) ? 10 : 4;
			SetupPins();
		}

		public virtual void SetupPins() { }
		public virtual void Execute(Circuit sim) { }

		public virtual bool needsBits() {
			return false;
		}

		public override void SetVoltageSource(int j, int voltageSourceNdx) {
			for(int i = 0; i != GetLeadCount(); i++) {
				Pin p = pins[i];
				if(p.output && j-- == 0)
					p.VoltageSource = voltageSourceNdx;
			}
		}

		public override void Stamp(Circuit simulation) {
			for(int i = 0; i != GetLeadCount(); i++) {
				Pin p = pins[i];
				if(p.output)
					simulation.StampVoltageSource(0, LeadNode[i], p.VoltageSource);
			}
		}

		public override void Step(Circuit simulation) {
			for(int i = 0; i != GetLeadCount(); i++) {
				Pin p = pins[i];
				if(!p.output)
					p.value = VoltageLead[i] > 2.5;
			}

			Execute(simulation);

			for(int i = 0; i != GetLeadCount(); i++) {
				Pin p = pins[i];

				if(p.output) {
					//Debug.Log(i, p.name, p.value, p.VoltageSource);
					simulation.UpdateVoltageSource(0, i, p.VoltageSource, p.value ? 5 : 0);
				}
			}
			//Debug.Log("--");
		}

		public override void Reset() {
			for(int i = 0; i != GetLeadCount(); i++) {
				pins[i].value = false;
				VoltageLead[i] = 0;
			}
			lastClock = false;
		}

		/*public override void getInfo(String[] arr) {
			arr[0] = getChipName();
			int a = 1;
			for(int i = 0; i != getLeadCount(); i++) {

				Pin p = pins[i];
				if(arr[a] != null) {
					arr[a] += "; ";
				} else {
					arr[a] = "";
				}

				String t = "";
				if(p.lineOver) t += '\'';
				if(p.clock) t = "Clk";
				arr[a] += t + " = " + getVoltageText(LeadVoltage[i]);
				if(i % 2 == 1)
					a++;
			}
		}*/

		public override void SetCurrent(int voltageSourceNdx, double c) {
			for(int i = 0; i != GetLeadCount(); i++) {
				if(pins[i].output && pins[i].VoltageSource == voltageSourceNdx)
					pins[i].current = c;
			}
		}

		public virtual String GetChipName() {
			return "chip";
		}

		public override bool LeadsAreConnected(int n1, int n2) {
			return false;
		}

		public override bool LeadIsGround(int n1) {
			return pins[n1].output;
		}

		public class Pin {

			public Pin(String nm) {
				name = nm;
			}

			public string name;

			public int VoltageSource;
			public bool lineOver, clock, output, value;
			public double current;
		}

	}
}