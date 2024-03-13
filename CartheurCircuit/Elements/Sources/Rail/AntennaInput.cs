using System;

namespace CartheurCircuit.Elements.Sources.Rail
{
    public class AntennaInput : VoltageInput
    {
        private double _fmphase;

        public AntennaInput()
            : base(WaveType.DC)
        { }

        public override void Stamp(Circuit simulation)
        {
            simulation.StampVoltageSource(0, LeadNode[0], VoltageSource);
        }

        public override void Step(Circuit simulation)
        {
            simulation.UpdateVoltageSource(0, LeadNode[0], VoltageSource, GetVoltage(simulation));
        }

        protected override double GetVoltage(Circuit sim)
        {
            _fmphase += 2 * Pi * (2200 + Math.Sin(2 * Pi * sim.Time * 13) * 100) * sim.TimeStep;
            double fm = 3 * Math.Sin(_fmphase);
            return Math.Sin(2 * Pi * sim.Time * 3000)
                    * (1.3 + Math.Sin(2 * Pi * sim.Time * 12)) * 3
                    + Math.Sin(2 * Pi * sim.Time * 2710)
                    * (1.3 + Math.Sin(2 * Pi * sim.Time * 13)) * 3
                    + Math.Sin(2 * Pi * sim.Time * 2433)
                    * (1.3 + Math.Sin(2 * Pi * sim.Time * 14)) * 3 + fm;
        }

    }
}
