using System;

namespace CartheurCircuit
{
    public class Lamp : CircuitElement
    {
        private double resistance;
        public static readonly double roomTemp = 300;

        public Lead LeadIn { get { return LeadZero; } }
        public Lead LeadOut { get { return LeadOne; } }

        /// <summary>
        /// Tempature
        /// </summary>
        public double Temperature { get; private set; }

        /// <summary>
        /// Nominal Power
        /// </summary>
        public double NominalPower { get; set; }

        /// <summary>
        /// Nominal Voltage
        /// </summary>
        public double NominalVoltage { get; set; }

        /// <summary>
        /// Warmup Time (s)
        /// </summary>
        public double WarmupTime { get; set; }

        /// <summary>
        /// Cooldown Time (s)
        /// </summary>
        public double CooldownTime { get; set; }

        public Lamp() : base()
        {
            Temperature = roomTemp;
            NominalPower = 100;
            NominalVoltage = 120;
            WarmupTime = 0.4;
            CooldownTime = 0.4;
        }

        public override void Reset()
        {
            base.Reset();
            Temperature = roomTemp;
        }

        public override void CalculateCurrent()
        {
            Current = (VoltageLead[0] - VoltageLead[1]) / resistance;
            // System.out.print(this + " res current set to " + current + "\n");
        }

        public override void Stamp(Circuit simulation)
        {
            simulation.StampNonLinear(LeadNode[0]);
            simulation.StampNonLinear(LeadNode[1]);
        }

        public override bool NonLinear() { return true; }

        public override void BeginStep(Circuit simulation)
        {
            // based on http://www.intusoft.com/nlpdf/nl11.pdf
            double nom_r = NominalVoltage * NominalVoltage / NominalPower;
            // this formula doesn't work for values over 5390
            double tp = (Temperature > 5390) ? 5390 : Temperature;
            resistance = nom_r * (1.26104 - 4.90662 * Math.Sqrt(17.1839 / tp - 0.00318794) - 7.8569 / (tp - 187.56));
            double cap = 1.57e-4 * NominalPower;
            double capw = cap * WarmupTime / .4;
            double capc = cap * CooldownTime / .4;
            // System.out.println(nom_r + " " + (resistance/nom_r));
            double voltageDiff = VoltageLead[0] - VoltageLead[1];
            double power = voltageDiff * Current;
            Temperature += power * simulation.TimeStep / capw;
            double cr = 2600 / NominalPower;
            Temperature -= simulation.TimeStep * (Temperature - roomTemp) / (capc * cr);
            // System.out.println(capw + " " + capc + " " + temp + " " +resistance);
        }

        public override void Step(Circuit simulation)
        {
            simulation.StampResistor(LeadNode[0], LeadNode[1], resistance);
        }

        public override void GetInfo(string[] arr)
        {
            arr[0] = "lamp";
            GetBasicInfo(arr);
            arr[3] = "R = " + CircuitUtilities.GetUnitText(resistance, Circuit.OhmString);
            arr[4] = "P = " + CircuitUtilities.GetUnitText(GetPower(), "W");
            arr[5] = "T = " + ((int)Temperature) + " K";
        }

    }
}