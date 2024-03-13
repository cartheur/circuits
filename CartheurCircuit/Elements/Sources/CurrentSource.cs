namespace CartheurCircuit.Elements.Sources
{
    public class CurrentSource : CircuitElement
    {
        public Lead LeadInput { get { return LeadZero; } }
        public Lead LeadOutput { get { return LeadOne; } }

        /// <summary>
        /// Current in amperes (A).
        /// </summary>
        public double SourceCurrent { get; set; } // Also voltage?!
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        public CurrentSource()
        {
            SourceCurrent = 0.01;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="value">The current source value.</param>
        public CurrentSource(double value)
        {
            SourceCurrent = value;
        }
        /// <summary>
        /// Stamps the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Stamp(Circuit simulation)
        {
            simulation.StampCurrentSource(LeadNode[0], LeadNode[1], SourceCurrent);
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = "current source";
            getBasicInfo(arr);
        }*/

        public override double GetVoltageDelta()
        {
            return VoltageLead[1] - VoltageLead[0];
        }
    }
}
