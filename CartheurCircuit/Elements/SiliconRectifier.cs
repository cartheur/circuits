using System;

namespace CartheurCircuit.Elements
{
    // Silicon-Controlled Rectifier
    // 3 nodes, 1 internal node
    // 0 = anode, 1 = cathode, 2 = gate
    // 0, 3 = variable resistor
    // 3, 2 = diode
    // 2, 1 = 50 ohm resistor

    public class SiliconRectifier : CircuitElement
    {
        private static readonly int anode = 0;
        private static readonly int cnode = 1;
        private static readonly int gnode = 2;
        private static readonly int inode = 3;

        public Lead LeadIn { get { return LeadZero; } }
        public Lead LeadOut { get { return LeadOne; } }
        public Lead LeadGate { get { return new Lead(this, 2); } }

        /// <summary>
        /// Gate-Cathode Resistance (ohms)
        /// </summary>
        public double GateCathodeResistance{ get; set; }

        /// <summary>
        /// Trigger Current (A)
        /// </summary>
        public double TriggerCurrent { get; set; }

        /// <summary>
        /// Holding Current (A)
        /// </summary>
        public double HoldingCurrent { get; set; }
        public double[] LeadVoltage { get; private set; }

        private Diode diode;
        private double ia, ic, ig;
        private double lastvac;
        private double lastvag;

        public SiliconRectifier()
            : base()
        {
            diode = new Diode();
            diode.Setup(0.8, 0);
            GateCathodeResistance = 50;
            HoldingCurrent = 0.0082;
            TriggerCurrent = 0.01;
        }

        public override bool NonLinear() { return true; }

        public override void Reset()
        {
            VoltageLead[anode] = VoltageLead[cnode] = VoltageLead[gnode] = 0;
            diode.Reset();
            lastvag = lastvac = 0;
        }

        public override int GetLeadCount()
        {
            return 3;
        }

        public override int GetInternalLeadCount()
        {
            return 1;
        }

        public override double GetPower()
        {
            return (LeadVoltage[anode] - LeadVoltage[gnode]) * ia + (LeadVoltage[cnode] - LeadVoltage[gnode]) * ic;
        }

        public double aresistance;

        public override void Stamp(Circuit simulation)
        {
            simulation.StampNonLinear(LeadNode[anode]);
            simulation.StampNonLinear(LeadNode[cnode]);
            simulation.StampNonLinear(LeadNode[gnode]);
            simulation.StampNonLinear(LeadNode[inode]);
            simulation.StampResistor(LeadNode[gnode], LeadNode[cnode], GateCathodeResistance);
            diode.Stamp(simulation, LeadNode[inode], LeadNode[gnode]);
        }

        public override void Step(Circuit simulation)
        {
            double vac = VoltageLead[anode] - VoltageLead[cnode]; // typically negative
            double vag = VoltageLead[anode] - VoltageLead[gnode]; // typically positive
            if (Math.Abs(vac - lastvac) > 0.01 || Math.Abs(vag - lastvag) > .01)
                simulation.Converged = false;
            lastvac = vac;
            lastvag = vag;
            diode.DoStep(simulation, VoltageLead[inode] - VoltageLead[gnode]);
            double icmult = 1 / TriggerCurrent;
            double iamult = 1 / HoldingCurrent - icmult;
            aresistance = (-icmult * ic + ia * iamult > 1) ? 0.0105 : 10E5;
            simulation.StampResistor(LeadNode[anode], LeadNode[inode], aresistance);
        }

        public string[] GetInfo()
        {
            String[] arr = new string[6];
            arr[0] = "SCR";
            double vac = VoltageLead[anode] - VoltageLead[cnode];
            double vag = VoltageLead[anode] - VoltageLead[gnode];
            double vgc = VoltageLead[gnode] - VoltageLead[cnode];
            arr[1] = "Ia = " + SiUnits.Current(ia);
            arr[2] = "Ig = " + SiUnits.Current(ig);
            arr[3] = "Vac = " + SiUnits.Voltage(vac);
            arr[4] = "Vag = " + SiUnits.Voltage(vag);
            arr[5] = "Vgc = " + SiUnits.Voltage(vgc);
            return arr;
        }

        public override void CalculateCurrent()
        {
            ic = (VoltageLead[cnode] - VoltageLead[gnode]) / GateCathodeResistance;
            ia = (VoltageLead[anode] - VoltageLead[inode]) / aresistance;
            ig = -ic - ia;
        }
    }
}
