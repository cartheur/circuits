using System;

namespace CartheurCircuit.Elements
{
    public class Voltage : CircuitElement
    {
        public enum WaveType
        {
            DC,
            AC,
            SQUARE,
            TRIANGLE,
            SAWTOOTH,
            PULSE,
            VAR
        }

        public Voltage(WaveType wf)
        {
            Waveform = wf;
            MaxVoltage = 5;
            Frequency = 40;
            Reset();
        }
        /// <summary>
        /// Gets or sets the waveform by wavetype.
        /// </summary>
        /// <value>
        /// The waveform.
        /// </value>
        public WaveType Waveform
        {
            get
            {
                return _waveform;
            }
            set
            {
                WaveType ow = _waveform;
                _waveform = value;
                if (_waveform == WaveType.DC && ow != WaveType.DC)
                    Bias = 0;
            }
        }
        /// <summary>
        /// Gets or sets the frequency in Hz.
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        public double Frequency { get; set; }
        /// <summary>
        /// Gets or sets the phase offset in degrees.
        /// </summary>
        /// <value>
        /// The phase shift.
        /// </value>
        public double PhaseShift
        {
            get
            {
                return _phaseShift * 180 / Pi;
            }
            set
            {
                _phaseShift = value * Pi / 180;
            }
        }
        /// <summary>
        /// Gets or sets the duty cycle.
        /// </summary>
        /// <value>
        /// The duty cycle.
        /// </value>
        public double DutyCycle
        {
            get
            {
                return _dutyCycle * 10;
            }
            set
            {
                _dutyCycle = value * 0.01;
            }
        }
        /// <summary>
        /// The max voltage (AC) or flat voltage (DC)
        /// </summary>
        public double MaxVoltage { get; set; }
        /// <summary>
        /// DC Offset (V)
        /// </summary>
        public double Bias { get; set; }

        private WaveType _waveform;
        private double _frequency;
        private double _phaseShift;
        private double _dutyCycle = 0.5;

        protected double FreqTimeZero;

        public override void Reset()
        {
            FreqTimeZero = 0;
        }

        protected void SetFrequency(double newFreq, double timeStep, double time)
        {
            double oldfreq = _frequency;
            _frequency = newFreq;
            double maxfreq = 1 / (8 * timeStep);
            if (_frequency > maxfreq)
                _frequency = maxfreq;
            FreqTimeZero = time - oldfreq * (time - FreqTimeZero) / _frequency;
        }

        public double TriangleFunc(double x)
        {
            if (x < Pi) return x * (2 / Pi) - 1;
            return 1 - (x - Pi) * (2 / Pi);
        }

        public override void Stamp(Circuit simulation)
        {
            if (Waveform == WaveType.DC)
            {
                simulation.StampVoltageSource(LeadNode[0], LeadNode[1], VoltageSource, GetVoltage(simulation));
            }
            else
            {
                simulation.StampVoltageSource(LeadNode[0], LeadNode[1], VoltageSource);
            }
        }

        public override void Step(Circuit simulation)
        {
            if (Waveform != WaveType.DC)
                simulation.UpdateVoltageSource(LeadNode[0], LeadNode[1], VoltageSource, GetVoltage(simulation));
        }

        protected virtual double GetVoltage(Circuit sim)
        {
            SetFrequency(Frequency, sim.TimeStep, sim.Time);
            double w = 2 * Pi * (sim.Time - FreqTimeZero) * _frequency + _phaseShift;
            switch (Waveform)
            {
                case WaveType.DC: return MaxVoltage + Bias;
                case WaveType.AC: return Math.Sin(w) * MaxVoltage + Bias;
                case WaveType.SQUARE: return Bias + ((w % (2 * Pi) > (2 * Pi * _dutyCycle)) ? -MaxVoltage : MaxVoltage);
                case WaveType.TRIANGLE: return Bias + TriangleFunc(w % (2 * Pi)) * MaxVoltage;
                case WaveType.SAWTOOTH: return Bias + (w % (2 * Pi)) * (MaxVoltage / Pi) - MaxVoltage;
                case WaveType.PULSE: return ((w % (2 * Pi)) < 1) ? MaxVoltage + Bias : Bias;
                default: return 0;
            }
        }

        public override int GetVoltageSourceCount()
        {
            return 1;
        }

        public override double GetVoltageDelta()
        {
            return VoltageLead[1] - VoltageLead[0];
        }

        /*public override double getPower() {
            return -getVoltageDiff() * current;
        }*/

        /*public override void getInfo(String[] arr) {
            switch(waveform) {
                case WaveType.DC:
                case WaveType.VAR:
                    arr[0] = "voltage source";
                    break;
                case WaveType.AC:
                    arr[0] = "A/C source";
                    break;
                case WaveType.SQUARE:
                    arr[0] = "square wave gen";
                    break;
                case WaveType.PULSE:
                    arr[0] = "pulse gen";
                    break;
                case WaveType.SAWTOOTH:
                    arr[0] = "sawtooth gen";
                    break;
                case WaveType.TRIANGLE:
                    arr[0] = "triangle gen";
                    break;
            }
            arr[1] = "I = " + getCurrentText(current);
            arr[2] = ((this is RailElm) ? "V = " : "Vd = ") + getVoltageText(getVoltageDiff());
            if(waveform != WaveType.DC && waveform != WaveType.VAR) {
                arr[3] = "f = " + getUnitText(frequency, "Hz");
                arr[4] = "Vmax = " + getVoltageText(maxVoltage);
                int i = 5;
                if(bias != 0) {
                    arr[i++] = "Voff = " + getVoltageText(bias);
                } else if(frequency > 500) {
                    arr[i++] = "wavelength = " + getUnitText(2.9979e8 / frequency, "m");
                }
                arr[i++] = "P = " + getUnitText(getPower(), "W");
            }
        }*/

    }
}