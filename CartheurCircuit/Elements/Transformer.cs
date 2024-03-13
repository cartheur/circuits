using System;

namespace CartheurCircuit.Elements
{
    public class Transformer : CircuitElement
    {
        /// <summary>
        /// Primary Inductance (H)
        /// </summary>
        public double Inductance { get; set; }
        /// <summary>
        /// Ratio
        /// </summary>
        public double Ratio { get; set; }
        /// <summary>
        /// Coupling Coefficient
        /// </summary>
        public double CouplingCoefficient { get; set; }

        public bool IsTrapezoidal { get; set; }

        public double[] current;

        private double a1, a2, a3, a4;
        private double curSourceValue1, curSourceValue2;

        public Transformer()
        {
            IsTrapezoidal = true;
            Inductance = 4;
            Ratio = 1;
            CouplingCoefficient = 0.999;
            current = new double[2];
        }

        public override int GetLeadCount()
        {
            return 4;
        }

        public override void Reset()
        {
            current[0] = current[1] = VoltageLead[0] = VoltageLead[1] = VoltageLead[2] = VoltageLead[3] = 0;
        }

        public override void Stamp(Circuit simulation)
        {
            // equations for transformer:
            // v1 = L1 di1/dt + M di2/dt
            // v2 = M di1/dt + L2 di2/dt
            // we invert that to get:
            // di1/dt = a1 v1 + a2 v2
            // di2/dt = a3 v1 + a4 v2
            // integrate di1/dt using trapezoidal approx and we get:
            // i1(t2) = i1(t1) + dt/2 (i1(t1) + i1(t2))
            // = i1(t1) + a1 dt/2 v1(t1) + a2 dt/2 v2(t1) +
            // a1 dt/2 v1(t2) + a2 dt/2 v2(t2)
            // the norton equivalent of this for i1 is:
            // a. current source, I = i1(t1) + a1 dt/2 v1(t1) + a2 dt/2 v2(t1)
            // b. resistor, G = a1 dt/2
            // c. current source controlled by voltage v2, G = a2 dt/2
            // and for i2:
            // a. current source, I = i2(t1) + a3 dt/2 v1(t1) + a4 dt/2 v2(t1)
            // b. resistor, G = a3 dt/2
            // c. current source controlled by voltage v2, G = a4 dt/2
            //
            // For backward euler,
            //
            // i1(t2) = i1(t1) + a1 dt v1(t2) + a2 dt v2(t2)
            //
            // So the current source value is just i1(t1) and we use
            // dt instead of dt/2 for the resistor and VCCS.
            //
            // first winding goes from node 0 to 2, second is from 1 to 3
            double l1 = Inductance;
            double l2 = Inductance * Ratio * Ratio;
            double m = CouplingCoefficient * Math.Sqrt(l1 * l2);
            // build inverted matrix
            double deti = 1 / (l1 * l2 - m * m);
            double ts = IsTrapezoidal ? simulation.TimeStep / 2 : simulation.TimeStep;
            a1 = l2 * deti * ts; // we multiply dt/2 into a1..a4 here
            a2 = -m * deti * ts;
            a3 = -m * deti * ts;
            a4 = l1 * deti * ts;
            simulation.StampConductance(LeadNode[0], LeadNode[2], a1);
            simulation.StampVccs(LeadNode[0], LeadNode[2], LeadNode[1], LeadNode[3], a2);
            simulation.StampVccs(LeadNode[1], LeadNode[3], LeadNode[0], LeadNode[2], a3);
            simulation.StampConductance(LeadNode[1], LeadNode[3], a4);
            simulation.StampRightSide(LeadNode[0]);
            simulation.StampRightSide(LeadNode[1]);
            simulation.StampRightSide(LeadNode[2]);
            simulation.StampRightSide(LeadNode[3]);
        }

        public override void BeginStep(Circuit simulation)
        {
            double voltdiff1 = VoltageLead[0] - VoltageLead[2];
            double voltdiff2 = VoltageLead[1] - VoltageLead[3];
            if (IsTrapezoidal)
            {
                curSourceValue1 = voltdiff1 * a1 + voltdiff2 * a2 + current[0];
                curSourceValue2 = voltdiff1 * a3 + voltdiff2 * a4 + current[1];
            }
            else
            {
                curSourceValue1 = current[0];
                curSourceValue2 = current[1];
            }
        }

        public override void Step(Circuit simulation)
        {
            simulation.StampCurrentSource(LeadNode[0], LeadNode[2], curSourceValue1);
            simulation.StampCurrentSource(LeadNode[1], LeadNode[3], curSourceValue2);
        }

        public override void CalculateCurrent()
        {
            double voltdiff1 = VoltageLead[0] - VoltageLead[2];
            double voltdiff2 = VoltageLead[1] - VoltageLead[3];
            current[0] = voltdiff1 * a1 + voltdiff2 * a2 + curSourceValue1;
            current[1] = voltdiff1 * a3 + voltdiff2 * a4 + curSourceValue2;
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = "transformer";
            arr[1] = "L = " + getUnitText(inductance, "H");
            arr[2] = "Ratio = 1:" + ratio;
            arr[3] = "Vd1 = " + getVoltageText(LeadVoltage[0] - LeadVoltage[2]);
            arr[4] = "Vd2 = " + getVoltageText(LeadVoltage[1] - LeadVoltage[3]);
            arr[5] = "I1 = " + getCurrentText(current[0]);
            arr[6] = "I2 = " + getCurrentText(current[1]);
        }*/

        public override bool LeadsAreConnected(int n1, int n2)
        {
            if (ComparePair(n1, n2, 0, 2)) return true;
            if (ComparePair(n1, n2, 1, 3)) return true;
            return false;
        }

    }
}