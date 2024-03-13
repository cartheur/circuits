using System.Collections.Generic;

namespace CartheurCircuit.Elements
{
    // 0 = switch
    // 1 = switch end 1
    // 2 = switch end 2
    // ...
    // 3n   = coil
    // 3n+1 = coil
    // 3n+2 = end of coil resistor

    public class RelayElement : InductorElement
    {
        private readonly Inductor _inductor;
        double _inductance;
        double _onResistance;
        double _rOff;
        double _onCurrent;
        int _poleCount;
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayElement"/> class.
        /// </summary>
        /// <param name="coilPosts">The coil posts.</param>
        public RelayElement(ICollection<double> coilPosts)
        {
            _inductor = new Inductor();
            Inductance = 0.2;
            _inductor.Setup(Inductance, 0, true);
            OnCurrent = 0.02;
            OnResistance = 0.05;
            OffResistance = 1e6;
            CoilR = 20;
            _coilCurrent = 0;
            NumberOfPosts = coilPosts.Count;
            PoleCount = 1;
            SetupPoles();
        }
        /// <summary>
        /// Gets or sets the number of posts.
        /// </summary>
        /// <value>
        /// The number of posts.
        /// </value>
        public int NumberOfPosts { private get; set; }
        /// <summary>
        /// On Resistance (ohms)
        /// </summary>
        public new double Inductance
        {
            get
            {
                return _inductance;
            }
            set
            {
                _inductance = value;
                _inductor.Setup(_inductance, _coilCurrent, true);
            }
        }
        /// <summary>
        /// On Resistance (ohms)
        /// </summary>
        public double OnResistance
        {
            get
            {
                return _onResistance;
            }
            set
            {
                if (value > 0)
                    _onResistance = value;
            }
        }
        /// <summary>
        /// Off Resistance (ohms)
        /// </summary>
        public double OffResistance
        {
            get
            {
                return _rOff;
            }
            set
            {
                if (value > 0)
                    _rOff = value;
            }
        }
        /// <summary>
        /// On Current (A)
        /// </summary>
        public double OnCurrent
        {
            get
            {
                return _onCurrent;
            }
            set
            {
                if (value > 0)
                    _onCurrent = value;
            }
        }
        /// <summary>
        /// Number of Poles
        /// </summary>
        public int PoleCount
        {
            get
            {
                return _poleCount;
            }
            set
            {
                if (value >= 1)
                {
                    _poleCount = value;
                    SetupPoles();
                }
            }
        }
        /// <summary>
        /// Coil Resistance (ohms)
        /// </summary>
        public double CoilR { get; set; }

        double _coilCurrent;
        private double[] _switchCurrent, _switchCurrentCount;
        private double d_position;
        private int i_position;
        private int nSwitch0 = 0;
        private int nSwitch1 = 1;
        private int nSwitch2 = 2;
        private int nCoil1;

        

        public void SetupPoles()
        {
            nCoil1 = 3 * PoleCount;
            if (_switchCurrent == null || _switchCurrent.Length != PoleCount)
            {
                _switchCurrent = new double[PoleCount];
                _switchCurrentCount = new double[PoleCount];
            }
        }

        public override int GetLeadCount()
        {
            return 2 + PoleCount * 3;
        }

        public override void Reset()
        {
            base.Reset();
            _inductor.Reset();
            _coilCurrent = 0;

            for (int i = 0; i != PoleCount; i++)
                _switchCurrent[i] = _switchCurrentCount[i] = 0;

        }

        public override bool NonLinear()
        {
            return true;
        }

        #region check these overrides
        //public override void GetInfo(String[] arr)
        //{
        //    arr[0] = i_position == 0 ? "relay (off)" : i_position == 1 ? "relay (on)" : "relay";
        //    int i;
        //    int ln = 1;
        //    for (i = 0; i != poleCount; i++)
        //    {
        //        arr[ln++] = "I" + (i + 1) + " = " + getCurrentDText(switchCurrent[i]);
        //    }
        //    arr[ln++] = "coil I = " + getCurrentDText(coilCurrent);
        //    arr[ln++] = "coil Vd = " + getVoltageDText(volts[nCoil1] - volts[nCoil2]);
        //}
        //public ElementLead[] coilPosts;
        //private ElementLead[][] swposts;
        //public override int GetInternalNodeCount()
        //{
        //    return 1;
        //}
        //public override ElementLead GetLead(int n)
        //{
        //    if (n < 3 * poleCount)
        //    {
        //        double swposts = 0;
        //        return swposts[n / 3][n % 3];
        //    }

        //    return coilPosts[n - 3 * poleCount];
        //}

        //public override bool GetConnection(int n1, int n2)
        //{
        //    return (n1 / 3 == n2 / 3);
        //}

        //public override void DoStep()
        //{
        //    double voltdiff = volts[nCoil1] - volts[nCoil3];
        //    ind.DoStep(voltdiff);
        //    int p;
        //    for (p = 0; p != poleCount * 3; p += 3)
        //    {
        //        sim.stampResistor(nodes[nSwitch0 + p], nodes[nSwitch1 + p], i_position == 0 ? r_on : r_off);
        //        sim.stampResistor(nodes[nSwitch0 + p], nodes[nSwitch2 + p], i_position == 1 ? r_on : r_off);
        //    }
        //}

        //public override void CalculateCurrent()
        //{
        //    double voltdiff = volts[nCoil1] - volts[nCoil3];
        //    coilCurrent = ind.CalculateCurrent(voltdiff);

        //    // actually this isn't correct, since there is a small amount
        //    // of current through the switch when off
        //    int p;
        //    for (p = 0; p != poleCount; p++)
        //    {
        //        if (i_position == 2)
        //        {
        //            switchCurrent[p] = 0;
        //        }
        //        else
        //        {
        //            switchCurrent[p] = (volts[nSwitch0 + p * 3] - volts[nSwitch1 + p * 3 + i_position]) / r_on;
        //        }
        //    }
        //}

        //public override void Stamp()
        //{
        //    // inductor from coil post 1 to internal node
        //    var nodes = new[] { 0, 0 };
        //    ind.Stamp(nodes[nCoil1], nodes[nCoil3]);
        //    // resistor from internal node to coil post 2
        //    simulation.stampResistor(nodes[nCoil3], nodes[nCoil2], coilR);

        //    for (int i = 0; i != poleCount * 3; i++)
        //        sim.stampNonLinear(nodes[nSwitch0 + i]);

        //}

        //public override void StartIteration()
        //{
        //    ind.StartIteration(volts[nCoil1] - volts[nCoil3]);

        //    // magic value to balance operate speed with reset speed
        //    // semi-realistically
        //    double magic = 1.3;
        //    double pmult = Math.Sqrt(magic + 1);
        //    double p = coilCurrent * pmult / onCurrent;
        //    d_position = Math.Abs(p * p) - 1.3;
        //    if (d_position < 0)
        //    {
        //        d_position = 0;
        //    }
        //    if (d_position > 1)
        //    {
        //        d_position = 1;
        //    }
        //    if (d_position < 0.1)
        //    {
        //        i_position = 0;
        //    }
        //    else if (d_position > .9)
        //    {
        //        i_position = 1;
        //    }
        //    else
        //    {
        //        i_position = 2;
        //        // System.out.println("ind " + this + " " + current + " " + voltdiff);
        //    }
        //}

        #endregion

    }
}
