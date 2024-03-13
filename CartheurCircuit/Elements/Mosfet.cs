using System;
using CartheurCircuit;

namespace CartheurCircuit.Elements
{

    public class Mosfet : CircuitElement
    {
        private double _threshold;

        private bool pnp;
        private double lastv1;
        private double lastv2;
        private double ids;
        private int mode;
        private double gm;

        public Lead LeadGate { get { return LeadZero; } }
        public Lead LeadSource { get { return LeadOne; } }
        public Lead LeadDrain { get { return new Lead(this, 2); } }
        public double[] LeadVoltage { get; private set; }

        /// <summary>
        /// Threshold Voltage
        /// </summary>
        public double Threshold
        {
            get
            {
                return (pnp ? -1 : 1) * _threshold;
            }
            set
            {
                _threshold = (pnp ? -1 : 1) * value;
            }
        }

        public Mosfet(bool isPNP) : base()
        {
            pnp = isPNP;
            _threshold = getDefaultThreshold();
        }

        public virtual double getDefaultThreshold()
        {
            return 1.5;
        }

        public virtual double getBeta()
        {
            return 0.02;
        }

        public override bool NonLinear() { return true; }

        public override void Reset()
        {
            lastv1 = lastv2 = VoltageLead[0] = VoltageLead[1] = VoltageLead[2] = 0;
        }

        public override double GetCurrent()
        {
            return ids;
        }

        public override double GetPower()
        {
            return ids * (LeadVoltage[2] - LeadVoltage[1]);
        }

        public override int GetLeadCount()
        {
            return 3;
        }

        public override void Stamp(Circuit simulation)
        {
            simulation.StampNonLinear(LeadNode[1]);
            simulation.StampNonLinear(LeadNode[2]);
        }

        public override void Step(Circuit simulation)
        {
            double[] vs = new double[3];
            vs[0] = VoltageLead[0];
            vs[1] = VoltageLead[1];
            vs[2] = VoltageLead[2];
            if (vs[1] > lastv1 + .5) vs[1] = lastv1 + .5;
            if (vs[1] < lastv1 - .5) vs[1] = lastv1 - .5;
            if (vs[2] > lastv2 + .5) vs[2] = lastv2 + .5;
            if (vs[2] < lastv2 - .5) vs[2] = lastv2 - .5;
            int source = 1;
            int drain = 2;
            if ((pnp ? -1 : 1) * vs[1] > (pnp ? -1 : 1) * vs[2])
            {
                source = 2;
                drain = 1;
            }
            int gate = 0;
            double vgs = vs[gate] - vs[source];
            double vds = vs[drain] - vs[source];
            if (Math.Abs(lastv1 - vs[1]) > .01 || Math.Abs(lastv2 - vs[2]) > .01)
                simulation.Converged = false;
            lastv1 = vs[1];
            lastv2 = vs[2];
            double realvgs = vgs;
            double realvds = vds;
            vgs *= (pnp ? -1 : 1);
            vds *= (pnp ? -1 : 1);
            ids = 0;
            gm = 0;
            double Gds = 0;
            double beta = getBeta();
            if (vgs > .5 && this is JfetElm)
            {
                simulation.Panic("JFET is reverse biased!", this);
                return;
            }
            if (vgs < _threshold)
            {
                // Should be all zero, but that causes a singular matrix, so instead we treat it as a large resistor.
                Gds = 1e-8;
                ids = vds * Gds;
                mode = 0;
            }
            else if (vds < vgs - _threshold)
            {
                // linear
                ids = beta * ((vgs - _threshold) * vds - vds * vds * .5);
                gm = beta * vds;
                Gds = beta * (vgs - vds - _threshold);
                mode = 1;
            }
            else
            {
                // saturation; Gds = 0
                gm = beta * (vgs - _threshold);
                // use very small Gds to avoid nonconvergence
                Gds = 1e-8;
                ids = 0.5 * beta * (vgs - _threshold) * (vgs - _threshold) + (vds - (vgs - _threshold)) * Gds;
                mode = 2;
            }
            double rs = -(pnp ? -1 : 1) * ids + Gds * realvds + gm * realvgs;
            simulation.StampMatrix(LeadNode[drain], LeadNode[drain], Gds);
            simulation.StampMatrix(LeadNode[drain], LeadNode[source], -Gds - gm);
            simulation.StampMatrix(LeadNode[drain], LeadNode[gate], gm);
            simulation.StampMatrix(LeadNode[source], LeadNode[drain], -Gds);
            simulation.StampMatrix(LeadNode[source], LeadNode[source], Gds + gm);
            simulation.StampMatrix(LeadNode[source], LeadNode[gate], -gm);
            simulation.StampRightSide(LeadNode[drain], rs);
            simulation.StampRightSide(LeadNode[source], -rs);
            if (source == 2 && (pnp ? -1 : 1) == 1 || source == 1 && (pnp ? -1 : 1) == -1)
                ids = -ids;
        }

        //public void GetFetInfo(String[] arr, String n)
        //{
        //    arr[0] = (((pnp ? -1 : 1) == -1) ? "p-" : "n-") + n;
        //    arr[0] += " (Vt = " + CircuitUtilities.GetVoltageText((pnp ? -1 : 1) * _threshold) + ")";
        //    arr[1] = (((pnp ? -1 : 1) == 1) ? "Ids = " : "Isd = ") + CircuitUtilities.GetCurrentText(ids);
        //    arr[2] = "Vgs = " + CircuitUtilities.GetVoltageText(LeadVoltage[0] - LeadVoltage[(pnp ? -1 : 1) == -1 ? 2 : 1]);
        //    arr[3] = (((pnp ? -1 : 1) == 1) ? "Vds = " : "Vsd = ") + CircuitUtilities.GetVoltageText(LeadVoltage[2] - LeadVoltage[1]);
        //    arr[4] = (mode == 0) ? "off" : (mode == 1) ? "linear" : "saturation";
        //    arr[5] = "gm = " + CircuitUtilities.GetUnitText(gm, "A/V");
        //}

        public string GetState()
        {
            return (mode == 0) ? "off" : (mode == 1) ? "linear" : "saturation";
        }

        /*public override void getInfo(String[] arr) {
			getFetInfo(arr, "MOSFET");
		}*/

        public override double GetVoltageDelta()
        {
            return VoltageLead[2] - VoltageLead[1];
        }

        public override bool LeadsAreConnected(int n1, int n2)
        {
            return !(n1 == 0 || n2 == 0);
        }
    }
}
