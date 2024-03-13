namespace CartheurCircuit
{
    public class Capacitor : CircuitElement
    {
        private double _capacitance = 1E-5;
        private double compResistance;
        private double voltdiff;
        private double curSourceValue;

        public bool IsTrapezoidal { get; set; }
        public Lead LeadIn { get { return LeadZero; } }
        public Lead LeadOut { get { return LeadOne; } }

        /// <summary>
        /// Capacitance (F)
        /// </summary>
        /// <remarks>Get the capacitance. For the special case of a parallel plate capacitor this is given by the formula C = epsilon0 * A / d Farads, where epsilon0 is the permittivity of free space or a vacuum with a value of 8.854e-12, A is the area across the face of the conductors and d is the distance between the plates.</remarks>
        public double Capacitance
        {
            get
            {
                return _capacitance;
            }
            set
            {
                if (value > 0)
                    _capacitance = value;
            }
        }

        public Capacitor() : base()
        {
            IsTrapezoidal = true;
        }

        public Capacitor(double c) : base()
        {
            Capacitance = c;
            IsTrapezoidal = true;
        }

        public override void SetLeadVoltage(int leadX, double vValue)
        {
            base.SetLeadVoltage(leadX, vValue);
            voltdiff = VoltageLead[0] - VoltageLead[1];
        }

        public override void Reset()
        {
            Current = 0;
            // Put small charge on caps when reset to start oscillators
            voltdiff = 1E-3;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulation">The circuit passed for simulation.</param>
        /// <remarks>Capacitor companion model using trapezoidal approximation (Norton equivalent) consists of a current source in parallel with a resistor. Trapezoidal is more accurate than backward euler but can cause oscillatory behavior, if RC is small relative to the timestep.</remarks>
        public override void Stamp(Circuit simulation)
        {
            if (IsTrapezoidal)
            {
                compResistance = simulation.TimeStep / (2 * Capacitance);
            }
            else
            {
                compResistance = simulation.TimeStep / Capacitance;
            }
            simulation.StampResistor(LeadNode[0], LeadNode[1], compResistance);
            simulation.StampRightSide(LeadNode[0]);
            simulation.StampRightSide(LeadNode[1]);
        }

        public override void BeginStep(Circuit simulation)
        {
            if (IsTrapezoidal)
            {
                curSourceValue = -voltdiff / compResistance - Current;
            }
            else
            {
                curSourceValue = -voltdiff / compResistance;
                // System.out.println("cap " + compResistance + " " + curSourceValue + " " + current + " " + voltdiff);
            }
        }

        public override void CalculateCurrent()
        {
            double voltdiff = VoltageLead[0] - VoltageLead[1];
            // We check compResistance because this might get called before stamp(CirSim sim), which sets compResistance, causing infinite current.
            if (compResistance > 0)
                Current = voltdiff / compResistance + curSourceValue;
        }

        public override void Step(Circuit simulation)
        {
            simulation.StampCurrentSource(LeadNode[0], LeadNode[1], curSourceValue);
        }

        public override void GetInfo(string[] arr)
        {
            arr[0] = "capacitor";
            GetBasicInfo(arr);
            arr[3] = "C = " + CircuitUtilities.GetUnitText(Capacitance, "F");
            arr[4] = "P = " + CircuitUtilities.GetUnitText(GetPower(), "W");
            double v = CircuitUtilities.GetVoltageDifference();
            arr[4] = "U = " + CircuitUtilities.GetUnitText(.5 * Capacitance * v * v, "J");
        }
    }
}
