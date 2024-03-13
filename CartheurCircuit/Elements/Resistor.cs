namespace CartheurCircuit.Elements
{
    public class Resistor : CircuitElement
    {
        /// <summary>
        /// Gets the lead input.
        /// </summary>
        /// <value>
        /// The lead input.
        /// </value>
        public Lead LeadInput { get { return LeadZero; } }
        /// <summary>
        /// Gets the lead output.
        /// </summary>
        /// <value>
        /// The lead output.
        /// </value>
        public Lead LeadOutput { get { return LeadOne; } }
        /// <summary>
        /// Resistance (ohms)
        /// </summary>
        public double Resistance { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        public Resistor()
        {
            Resistance = 100;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="value">The resistor value.</param>
        public Resistor(double value)
        {
            Resistance = value;
        }
        /// <summary>
        /// Calculates the current.
        /// </summary>
        public override void CalculateCurrent()
        {
            Current = (VoltageLead[0] - VoltageLead[1]) / Resistance;
        }
        /// <summary>
        /// Stamps the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Stamp(Circuit simulation)
        {
            simulation.StampResistor(LeadNode[0], LeadNode[1], Resistance);
        }

        //public override void GetInfo(string[] arr) {
        //    arr[0] = "resistor";
        //    GetBasicInfo(arr);
        //    arr[3] = "R = " + getUnitText(resistance, Circuit.ohmString);
        //    arr[4] = "P = " + getUnitText(getPower(), "W");
        //}

    }
}