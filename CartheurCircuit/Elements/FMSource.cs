using System;

namespace CartheurCircuit
{
    public class FMSource : CircuitElement
    {
        private double freqTimeZero;
        private double lasttime = 0;
        private double funcx = 0;
        /// <summary>
        /// The lead out of the circuit.
        /// </summary>
        public Lead LeadOut { get { return LeadZero; } }

        /// <summary>
        /// Carrier Frequency (Hz)
        /// </summary>
        public double CarrierFrequency { get; set; }

        /// <summary>
        /// The signalfreq.
        /// </summary>
        public double SignalFrequency { get; set; }

        /// <summary>
        /// Max Voltage
        /// </summary>
        public double MaxVoltage { get; set; }

        /// <summary>
        /// Deviation (Hz)
        /// </summary>
        public double Deviation { get; set; }

        public FMSource() : base()
        {
            Deviation = 200;
            MaxVoltage = 5;
            CarrierFrequency = 800;
            SignalFrequency = 40;
            Reset();
        }

        public override void Reset()
        {
            freqTimeZero = 0;
        }

        public override int GetLeadCount()
        {
            return 1;
        }

        public override void Stamp(Circuit simulation)
        {
            simulation.StampVoltageSource(0, LeadNode[0], VoltageSource);
        }

        public override void Step(Circuit simulation)
        {
            simulation.UpdateVoltageSource(0, LeadNode[0], VoltageSource, GetVoltage(simulation.Time));
        }

        public double GetVoltage(double time)
        {
            double deltaT = time - lasttime;
            lasttime = time;
            double signalamplitude = Math.Sin((2 * Pi * (time - freqTimeZero)) * SignalFrequency);
            funcx += deltaT * (CarrierFrequency + (signalamplitude * Deviation));
            double w = 2 * Pi * funcx;
            return Math.Sin(w) * MaxVoltage;
        }

        public override double GetVoltageDelta()
        {
            return VoltageLead[0];
        }

        public override bool LeadIsGround(int n1)
        {
            return true;
        }

        public override int GetVoltageSourceCount()
        {
            return 1;
        }

        public override double GetPower()
        {
            return -CircuitUtilities.GetVoltageDifference() * Current;
        }

        public override void GetInfo(string[] arr)
        {
            arr[0] = "FM Source";
            arr[1] = "I = " + CircuitUtilities.GetCurrentText(Current);
            arr[2] = "V = " + CircuitUtilities.GetVoltageText(CircuitUtilities.GetVoltageDifference().ToString());
            arr[3] = "cf = " + CircuitUtilities.GetUnitText(CarrierFrequency, "Hz");
            arr[4] = "sf = " + CircuitUtilities.GetUnitText(SignalFrequency, "Hz");
            arr[5] = "dev =" + CircuitUtilities.GetUnitText(Deviation, "Hz");
            arr[6] = "Vmax = " + CircuitUtilities.GetVoltageText(MaxVoltage.ToString());
        }

    }
}