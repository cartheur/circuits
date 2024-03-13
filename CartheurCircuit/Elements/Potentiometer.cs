namespace CartheurCircuit.Elements
{
    public class Potentiometer : CircuitElement
    {
        private double _resistance1;
        private double _resistance2;
        private double _current1;
        private double _current2;
        private double _current3;

        public Lead LeadOutput { get { return LeadZero; } }
        public Lead LeadInput { get { return LeadOne; } }
        public Lead LeadVoltage { get { return new Lead(this, 2); } }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public double Position { get; set; }
        /// <summary>
        /// Resistance (ohms)
        /// </summary>
        public double MaxResistance { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Potentiometer"/> class.
        /// </summary>
        public Potentiometer()
        {
            MaxResistance = 1000;
            Position = 0.5;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Potentiometer"/> class.
        /// </summary>
        /// <param name="value">The value in ohms.</param>
        public Potentiometer(double value)
        {
            MaxResistance = value;
        }
        /// <summary>
        /// Gets the number of leads.
        /// </summary>
        /// <returns></returns>
        public override int GetLeadCount()
        {
            return 3;
        }
        /// <summary>
        /// Calculates the current.
        /// </summary>
        public override void CalculateCurrent()
        {
            _current1 = (VoltageLead[0] - VoltageLead[2]) / _resistance1;
            _current2 = (VoltageLead[1] - VoltageLead[2]) / _resistance2;
            _current3 = -_current1 - _current2;
        }
        /// <summary>
        /// Stamps the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Stamp(Circuit simulation)
        {
            _resistance1 = MaxResistance * Position;
            _resistance2 = MaxResistance * (1 - Position);
            simulation.StampResistor(LeadNode[0], LeadNode[2], _resistance1);
            simulation.StampResistor(LeadNode[2], LeadNode[1], _resistance2);
        }

        /*public override void getInfo(String[] arr) {
            arr[0] = "potentiometer";
            //arr[1] = "Vd = " + getVoltageDText(getVoltageDiff());
            arr[2] = "R1 = " + getUnitText(resistance1, Circuit.ohmString);
            arr[3] = "R2 = " + getUnitText(resistance2, Circuit.ohmString);
            arr[4] = "I1 = " + getCurrentDText(current1);
            arr[5] = "I2 = " + getCurrentDText(current2);
        }*/

    }
}