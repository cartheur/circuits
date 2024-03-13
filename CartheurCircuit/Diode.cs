using System;

namespace CartheurCircuit
{
    public class Diode
    {
        private readonly int[] _nodes;
        const double Epsilon = 1E-6;
        public double Leakage = 1e-14; // was 1e-9;
        private double _vt, _vdcoef, _fwdrop, _zvoltage, _zoffset;
        private double _lastvoltdiff;
        private double _vcrit;

        public Diode()
        {
            _nodes = new int[2];
        }

        public void Setup(double fw, double zv)
        {
            _fwdrop = fw;
            _zvoltage = zv;
            _vdcoef = Math.Log(1 / Leakage + 1) / _fwdrop;
            _vt = 1 / _vdcoef;
            // critical voltage for limiting; current is vt/sqrt(2) at this voltage
            _vcrit = _vt * Math.Log(_vt / (Math.Sqrt(2) * Leakage));
            if (Math.Abs(_zvoltage) < Epsilon)
            {
                _zoffset = 0;
            }
            else
            {
                // calculate offset which will give us 5mA at zvoltage
                const double i = -0.005;
                _zoffset = _zvoltage - Math.Log(-(1 + i / Leakage)) / _vdcoef;
            }
        }

        public void Reset()
        {
            _lastvoltdiff = 0;
        }

        public double LimitStep(Circuit sim, double vnew, double vold)
        {
            double arg;
            // check new voltage; has current changed by factor of e^2?
            if (vnew > _vcrit && Math.Abs(vnew - vold) > (_vt + _vt))
            {
                if (vold > 0)
                {
                    arg = 1 + (vnew - vold) / _vt;
                    if (arg > 0)
                    {
                        // adjust vnew so that the current is the same
                        // as in linearized model from previous iteration.
                        // current at vnew = old current * arg
                        vnew = vold + _vt * Math.Log(arg);
                        // current at v0 = 1uA
                        double v0 = Math.Log(1e-6 / Leakage) * _vt;
                        vnew = Math.Max(v0, vnew);
                    }
                    else
                    {
                        vnew = _vcrit;
                    }
                }
                else
                {
                    // adjust vnew so that the current is the same
                    // as in linearized model from previous iteration.
                    // (1/vt = slope of load line)
                    vnew = _vt * Math.Log(vnew / _vt);
                }
                sim.Converged = false;
                // System.out.println(vnew + " " + oo + " " + vold);
            }
            else if (vnew < 0 && Math.Abs(_zoffset) > Epsilon)
            {
                // for Zener breakdown, use the same logic but translate the values
                vnew = -vnew - _zoffset;
                vold = -vold - _zoffset;
                if (vnew > _vcrit && Math.Abs(vnew - vold) > (_vt + _vt))
                {
                    if (vold > 0)
                    {
                        arg = 1 + (vnew - vold) / _vt;
                        if (arg > 0)
                        {
                            vnew = vold + _vt * Math.Log(arg);
                            double v0 = Math.Log(1e-6 / Leakage) * _vt;
                            vnew = Math.Max(v0, vnew);
                            // System.out.println(oo + " " + vnew);
                        }
                        else
                        {
                            vnew = _vcrit;
                        }
                    }
                    else
                    {
                        vnew = _vt * Math.Log(vnew / _vt);
                    }
                    sim.Converged = false;
                }
                vnew = -(vnew + _zoffset);
            }
            return vnew;
        }

        public void Stamp(Circuit sim, int n0, int n1)
        {
            _nodes[0] = n0;
            _nodes[1] = n1;
            sim.StampNonLinear(_nodes[0]);
            sim.StampNonLinear(_nodes[1]);
        }

        public void DoStep(Circuit sim, double voltdiff)
        {
            // used to have .1 here, but needed .01 for peak detector
            if (Math.Abs(voltdiff - _lastvoltdiff) > 0.01)
                sim.Converged = false;

            voltdiff = LimitStep(sim, voltdiff, _lastvoltdiff);
            _lastvoltdiff = voltdiff;

            if (voltdiff >= 0 || Math.Abs(_zvoltage) < Epsilon)
            {
                // regular diode or forward-biased zener
                double eval = Math.Exp(voltdiff * _vdcoef);
                // make diode linear with negative voltages; aids convergence
                if (voltdiff < 0)
                    eval = 1;

                double geq = _vdcoef * Leakage * eval;
                double nc = (eval - 1) * Leakage - geq * voltdiff;
                sim.StampConductance(_nodes[0], _nodes[1], geq);
                sim.StampCurrentSource(_nodes[0], _nodes[1], nc);
            }
            else
            {
                // Zener diode
                // I(Vd) = Is * (exp[Vd*C] - exp[(-Vd-Vz)*C] - 1 )
                // geq is I'(Vd) nc is I(Vd) + I'(Vd)*(-Vd)
                double geq = Leakage * _vdcoef * (Math.Exp(voltdiff * _vdcoef) + Math.Exp((-voltdiff - _zoffset) * _vdcoef));
                double nc = Leakage * (Math.Exp(voltdiff * _vdcoef) - Math.Exp((-voltdiff - _zoffset) * _vdcoef) - 1) + geq * (-voltdiff);
                sim.StampConductance(_nodes[0], _nodes[1], geq);
                sim.StampCurrentSource(_nodes[0], _nodes[1], nc);
            }
        }

        public double CalculateCurrent(double voltdiff)
        {
            if (voltdiff >= 0 || Math.Abs(_zvoltage) < Epsilon)
                return Leakage * (Math.Exp(voltdiff * _vdcoef) - 1);

            return Leakage * (Math.Exp(voltdiff * _vdcoef) - Math.Exp((-voltdiff - _zoffset) * _vdcoef) - 1);
        }
    }
}