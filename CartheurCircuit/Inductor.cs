//Orignal .cs file has a pragma if DEPRECIATED for this class.

namespace CartheurCircuit
{
    public class Inductor
    {
        public bool IsTrapezoidal { get; set; }

        public int[] Nodes;

        double _current;
        double _inductance = 1E-6;
        double _compResistance;
        double _currentSourceValue;

        public Inductor()
        {
            Nodes = new int[2];
        }

        public void Setup(double inductance, double current, bool isTrapezoid)
        {
            _inductance = inductance;
            _current = current;
            IsTrapezoidal = isTrapezoid;
        }

        public void Reset()
        {
            _current = 0;
        }
        /// <summary>
        /// Stamps the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="deltaTime">The delta time.</param>
        /// <param name="n0">The n0.</param>
        /// <param name="n1">The n1.</param>
        /// <remarks>
        /// The inductor companion model using trapezoidal or backward Euler approximations (Norton equivalent) consists of a current source in parallel with a resistor. Trapezoidal is more accurate than backward euler but can cause oscillatory behavior. The oscillation is a real problem in circuits with switches.
        /// </remarks>
        public void Stamp(Circuit simulation, double deltaTime, int n0, int n1)
        {
            Nodes[0] = n0;
            Nodes[1] = n1;
            if (IsTrapezoidal)
            {
                _compResistance = 2 * _inductance / deltaTime;
            }
            else
            {
                _compResistance = _inductance / deltaTime; // backward euler
            }
            simulation.StampResistor(Nodes[0], Nodes[1], _compResistance);
            simulation.StampRightSide(Nodes[0]);
            simulation.StampRightSide(Nodes[1]);
        }

        public void StartIteration(double voltdiff)
        {
            if (IsTrapezoidal)
            {
                _currentSourceValue = voltdiff / _compResistance + _current;
            }
            else
            {
                _currentSourceValue = _current; // backward euler
            }
        }
        /// <summary>
        /// Calculates the current.
        /// </summary>
        /// <param name="voltdiff">The voltdiff.</param>
        /// <returns></returns>
        /// <remarks>Check compResistance because this might get called before Stamp(CirSim sim), which sets compResistance, causing infinite current.</remarks>
        public double CalculateCurrent(double voltdiff)
        {
            if (_compResistance > 0)
                _current = voltdiff / _compResistance + _currentSourceValue;
            return _current;
        }

        public void DoStep(Circuit sim, double voltdiff)
        {
            sim.StampCurrentSource(Nodes[0], Nodes[1], _currentSourceValue);
        }
    }
}
