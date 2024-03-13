namespace CartheurCircuit.Elements
{
    public class TappedTransformer : CircuitElement
    {
        /// <summary>
        /// Primary Inductance (H)
        /// </summary>
        public double Inductance { get; set; }

        /// <summary>
        /// Ratio
        /// </summary>
        public double Ratio { get; set; }

       public double[] current;

        private double[] a;
        private double[] curSourceValue, voltdiff;

        public TappedTransformer()
            : base()
        {
            Inductance = 4;
            Ratio = 1;
            current = new double[4];
            voltdiff = new double[3];
            curSourceValue = new double[3];
            a = new double[9];
        }

        public override int GetLeadCount()
        {
            return 5;
        }

        public override void Reset()
        {
            current[0] = current[1] = VoltageLead[0] = VoltageLead[1] = VoltageLead[2] = VoltageLead[3] = 0;
        }

        public override void Stamp(Circuit simulation)
        {
            // equations for transformer:
            // v1 = L1 di1/dt + M1 di2/dt + M1 di3/dt
            // v2 = M1 di1/dt + L2 di2/dt + M2 di3/dt
            // v3 = M1 di1/dt + M2 di2/dt + L2 di3/dt
            // we invert that to get:
            // di1/dt = a1 v1 + a2 v2 + a3 v3
            // di2/dt = a4 v1 + a5 v2 + a6 v3
            // di3/dt = a7 v1 + a8 v2 + a9 v3
            // integrate di1/dt using trapezoidal approx and we get:
            // i1(t2) = i1(t1) + dt/2 (i1(t1) + i1(t2))
            // = i1(t1) + a1 dt/2 v1(t1)+a2 dt/2 v2(t1)+a3 dt/2 v3(t3) +
            // a1 dt/2 v1(t2)+a2 dt/2 v2(t2)+a3 dt/2 v3(t3)
            // the norton equivalent of this for i1 is:
            // a. current source, I = i1(t1) + a1 dt/2 v1(t1) + a2 dt/2 v2(t1)
            // + a3 dt/2 v3(t1)
            // b. resistor, G = a1 dt/2
            // c. current source controlled by voltage v2, G = a2 dt/2
            // d. current source controlled by voltage v3, G = a3 dt/2
            // and similarly for i2
            //
            // first winding goes from node 0 to 1, second is from 2 to 3 to 4
            double l1 = Inductance;
            double cc = .99;
            // double m1 = .999*Math.sqrt(l1*l2);
            // mutual inductance between two halves of the second winding
            // is equal to self-inductance of either half (slightly less
            // because the coupling is not perfect)
            // double m2 = .999*l2;
            // load pre-inverted matrix
            a[0] = (1 + cc) / (l1 * (1 + cc - 2 * cc * cc));
            a[1] = a[2] = a[3] = a[6] = 2 * cc / ((2 * cc * cc - cc - 1) * Inductance * Ratio);
            a[4] = a[8] = -4 * (1 + cc) / ((2 * cc * cc - cc - 1) * l1 * Ratio * Ratio);
            a[5] = a[7] = 4 * cc / ((2 * cc * cc - cc - 1) * l1 * Ratio * Ratio);
            int i;
            for (i = 0; i != 9; i++)
                a[i] *= simulation.TimeStep / 2;

            simulation.StampConductance(LeadNode[0], LeadNode[1], a[0]);
            simulation.StampVccs(LeadNode[0], LeadNode[1], LeadNode[2], LeadNode[3], a[1]);
            simulation.StampVccs(LeadNode[0], LeadNode[1], LeadNode[3], LeadNode[4], a[2]);

            simulation.StampVccs(LeadNode[2], LeadNode[3], LeadNode[0], LeadNode[1], a[3]);
            simulation.StampConductance(LeadNode[2], LeadNode[3], a[4]);
            simulation.StampVccs(LeadNode[2], LeadNode[3], LeadNode[3], LeadNode[4], a[5]);

            simulation.StampVccs(LeadNode[3], LeadNode[4], LeadNode[0], LeadNode[1], a[6]);
            simulation.StampVccs(LeadNode[3], LeadNode[4], LeadNode[2], LeadNode[3], a[7]);
            simulation.StampConductance(LeadNode[3], LeadNode[4], a[8]);

            for (i = 0; i != 5; i++)
                simulation.StampRightSide(LeadNode[i]);
        }

        public override void BeginStep(Circuit simulation)
        {
            voltdiff[0] = VoltageLead[0] - VoltageLead[1];
            voltdiff[1] = VoltageLead[2] - VoltageLead[3];
            voltdiff[2] = VoltageLead[3] - VoltageLead[4];
            for (int i = 0; i != 3; i++)
            {
                curSourceValue[i] = current[i];
                for (int j = 0; j != 3; j++)
                    curSourceValue[i] += a[i * 3 + j] * voltdiff[j];
            }
        }

        public override void Step(Circuit simulation)
        {
            simulation.StampCurrentSource(LeadNode[0], LeadNode[1], curSourceValue[0]);
            simulation.StampCurrentSource(LeadNode[2], LeadNode[3], curSourceValue[1]);
            simulation.StampCurrentSource(LeadNode[3], LeadNode[4], curSourceValue[2]);
        }

        public override void CalculateCurrent()
        {
            voltdiff[0] = VoltageLead[0] - VoltageLead[1];
            voltdiff[1] = VoltageLead[2] - VoltageLead[3];
            voltdiff[2] = VoltageLead[3] - VoltageLead[4];
            for (int i = 0; i != 3; i++)
            {
                current[i] = curSourceValue[i];
                for (int j = 0; j != 3; j++)
                    current[i] += a[i * 3 + j] * voltdiff[j];
            }
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = "transformer";
            arr[1] = "L = " + getUnitText(inductance, "H");
            arr[2] = "Ratio = " + ratio;
            // arr[3] = "I1 = " + getCurrentText(current1);
            arr[3] = "Vd1 = " + getVoltageText(LeadVoltage[0] - LeadVoltage[2]);
            // arr[5] = "I2 = " + getCurrentText(current2);
            arr[4] = "Vd2 = " + getVoltageText(LeadVoltage[1] - LeadVoltage[3]);
        }*/

        public override bool LeadsAreConnected(int n1, int n2)
        {
            if (ComparePair(n1, n2, 0, 1)) return true;
            if (ComparePair(n1, n2, 2, 3)) return true;
            if (ComparePair(n1, n2, 3, 4)) return true;
            if (ComparePair(n1, n2, 2, 4)) return true;
            return false;
        }

    }
}