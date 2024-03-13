using System;

namespace CartheurCircuit.Elements
{
    public class OpAmp : CircuitElement
    {
        private double lastvd;
        private double gain;

        public Lead NegativeLead { get { return LeadZero; } }
        public Lead PositiveLead { get { return LeadOne; } }
        public Lead OutputLead { get { return new Lead(this, 2); } }

        System.Random random = new Random();

        /// <summary>
        /// Max Output (V)
        /// </summary>
        public double MaxOut { get; set; }

        /// <summary>
        /// Min Output (V)
        /// </summary>
        public double MinOut { get; set; }

        public OpAmp()
            : base()
        {
            MaxOut = 15;
            MinOut = -15;
            gain = 100000;
        }

        public OpAmp(bool lowGain)
            : base()
        {
            gain = (lowGain) ? 1000 : 100000;
        }

        public override bool NonLinear() { return true; }

        /*public override double getPower() {
            return LeadVoltage[2] * current;
        }*/

        public override int GetLeadCount()
        {
            return 3;
        }

        public override int GetVoltageSourceCount()
        {
            return 1;
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = "op-amp";
            arr[1] = "V+ = " + getVoltageText(LeadVoltage[1]);
            arr[2] = "V- = " + getVoltageText(LeadVoltage[0]);
            // sometimes the voltage goes slightly outside range, to make
            // convergence easier. so we hide that here.
            double vo = Math.Max(Math.Min(LeadVoltage[2], maxOut), minOut);
            arr[3] = "Vout = " + getVoltageText(vo);
            arr[4] = "Iout = " + getCurrentText(current);
            arr[5] = "range = " + getVoltageText(minOut) + " to " + getVoltageText(maxOut);
        }*/

        public override void Stamp(Circuit simulation)
        {
            int vn = simulation.NodeCount + VoltageSource;
            simulation.StampNonLinear(vn);
            simulation.StampMatrix(LeadNode[2], vn, 1);
        }

        public override void Step(Circuit simulation)
        {

            double vd = VoltageLead[1] - VoltageLead[0];
            if (Math.Abs(lastvd - vd) > 0.1)
            {
                simulation.Converged = false;
            }
            else if (VoltageLead[2] > MaxOut + 0.1 || VoltageLead[2] < MinOut - 0.1)
            {
                simulation.Converged = false;
            }

            double x = 0;
            int vn = simulation.NodeCount + VoltageSource;
            double dx = 0;
            if (vd >= MaxOut / gain && (lastvd >= 0 || getRand(4) == 1))
            {
                dx = 1E-4;
                x = MaxOut - dx * MaxOut / gain;
            }
            else if (vd <= MinOut / gain && (lastvd <= 0 || getRand(4) == 1))
            {
                dx = 1E-4;
                x = MinOut - dx * MinOut / gain;
            }
            else
            {
                dx = gain;
            }

            // newton-raphson
            simulation.StampMatrix(vn, LeadNode[0], dx);
            simulation.StampMatrix(vn, LeadNode[1], -dx);
            simulation.StampMatrix(vn, LeadNode[2], 1);
            simulation.StampRightSide(vn, x);

            lastvd = vd;
        }

        int getRand(int x)
        {
            int q = random.Next();
            if (q < 0) q = -q;
            return q % x;
        }

        // there is no current path through the op-amp inputs, but there is an indirect path through the output to ground.
        public override bool LeadsAreConnected(int n1, int n2)
        {
            return false;
        }

        public override bool LeadIsGround(int n1)
        {
            return (n1 == 2);
        }

        public override double GetVoltageDelta()
        {
            return VoltageLead[2] - VoltageLead[1];
        }

    }
}