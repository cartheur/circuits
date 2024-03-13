namespace CartheurCircuit.Elements
{
    public class InductorElement : CircuitElement
    {
        public Lead LeadInput { get { return LeadZero; } }
        public Lead LeadOutput { get { return LeadOne; } }
        /// <summary>
        /// The inductance value in Henries.
        /// </summary>
        public double Inductance { get; set; }
        public bool IsTrapezoidal { get; set; }

        readonly int[] _nodes;
        double _compResistance;
        double _currentSourceValue;
        /// <summary>
        /// Initializes a new instance of the <see cref="Inductor"/> class with a value of 1E-6H.
        /// </summary>
        public InductorElement()
        {
            _nodes = new int[2];
            Inductance = 1;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Inductor"/> class.
        /// </summary>
        /// <param name="inductance">The inductance value.</param>
        public InductorElement(double inductance)
        {
            _nodes = new int[2];
            Inductance = inductance;
        }
        /// <summary>
        /// Resets the current.
        /// </summary>
        public override void Reset()
        {
            Current = VoltageLead[0] = VoltageLead[1] = 0;
        }
        /// <summary>
        /// Stamps the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <remarks>The inductor companion model using trapezoidal or backward Euler approximations (Norton equivalent) consists of a current source in parallel with a resistor. Trapezoidal is more accurate than backward euler but can cause oscillatory behavior. The oscillation is a real problem in circuits with switches.</remarks>
        public override void Stamp(Circuit simulation)
        {
            _nodes[0] = LeadNode[0];
            _nodes[1] = LeadNode[1];
            if (IsTrapezoidal)
            {
                _compResistance = 2 * Inductance / simulation.TimeStep;
            }
            else
            {
                _compResistance = Inductance / simulation.TimeStep; // backward euler
            }
            simulation.StampResistor(_nodes[0], _nodes[1], _compResistance);
            simulation.StampRightSide(_nodes[0]);
            simulation.StampRightSide(_nodes[1]);
        }
        /// <summary>
        /// Begins the step.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void BeginStep(Circuit simulation)
        {
            double voltdiff = VoltageLead[0] - VoltageLead[1];
            if (IsTrapezoidal)
            {
                _currentSourceValue = voltdiff / _compResistance + Current;
            }
            else
            {
                _currentSourceValue = Current; // backward euler
            }
        }
        /// <summary>
        /// Is a nonlinear element.
        /// </summary>
        /// <returns></returns>
        public override bool NonLinear() { return true; }
        /// <summary>
        /// Calculates the current.
        /// </summary>
        public override void CalculateCurrent()
        {
            double voltdiff = VoltageLead[0] - VoltageLead[1];
            if (_compResistance > 0)
                Current = voltdiff / _compResistance + _currentSourceValue;
        }
        /// <summary>
        /// Sets the step in the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Step(Circuit simulation)
        {
            double voltdiff = VoltageLead[0] - VoltageLead[1];
            simulation.StampCurrentSource(_nodes[0], _nodes[1], _currentSourceValue);
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = "inductor";
            getBasicInfo(arr);
            arr[3] = "L = " + getUnitText(inductance, "H");
            arr[4] = "P = " + getUnitText(getPower(), "W");
        }*/

    }
}
