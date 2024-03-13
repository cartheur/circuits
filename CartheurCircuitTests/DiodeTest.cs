using System.Linq;
using CartheurCircuit;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;
using NUnit.Framework;

namespace AnalogCircuitTests
{
    [TestFixture]
    public class DiodeTest
    {
        [Test]
        public void SimpleDiodeTest()
        {
            var sim = new Circuit();

            var voltage0 = sim.Create<VoltageInput>(Voltage.WaveType.AC);
            var diode = sim.Create<DiodeElm>();
            var ground = sim.Create<Ground>();

            sim.Connect(voltage0.LeadPositive, diode.leadIn);
            sim.Connect(diode.leadOut, ground.leadIn);

            var diodeScope = sim.Watch(diode);

            var cycleTime = 1 / voltage0.Frequency;
            var quarterCycleTime = cycleTime / 4;

            var steps = (int)(cycleTime / sim.TimeStep);
            for (var x = 1; x <= steps; x++)
                sim.DoTick();

            var voltageHigh = diodeScope.Max((f) => f.Voltage);
            var voltageHighNdx = diodeScope.FindIndex((f) => f.Voltage == voltageHigh);

            TestUtilities.Compare(voltageHigh, voltage0.DutyCycle, 4);
            TestUtilities.Compare(diodeScope[voltageHighNdx].Time, quarterCycleTime, 4);

            var voltageLow = diodeScope.Min((f) => f.Voltage);
            var voltageLowNdx = diodeScope.FindIndex((f) => f.Voltage == voltageLow);

            TestUtilities.Compare(voltageLow, -voltage0.DutyCycle, 4);
            TestUtilities.Compare(diodeScope[voltageLowNdx].Time, quarterCycleTime * 3, 4);

            var currentHigh = diodeScope.Max((f) => f.Current);
            var currentHighNdx = diodeScope.FindIndex((f) => f.Current == currentHigh);
            TestUtilities.Compare(diodeScope[voltageHighNdx].Time, diodeScope[currentHighNdx].Time, 5);

            var currentLow = diodeScope.Min((f) => f.Current);
            var currentLowNdx = diodeScope.FindIndex((f) => f.Current == currentLow);

            TestUtilities.Compare(currentLow, 0, 8);
        }

        [Test]
        public void HalfWaveRectifierTest()
        {

            /*string nm = TestContext.CurrentContext.Test.Name;
            string js = System.IO.File.ReadAllText(string.Format("./{0}.json", nm));
            Circuit sim = JsonSerializer.DeserializeFromString<Circuit>(js);
            sim.needAnalyze();

            var source0 = sim.getElm(0) as VoltageElm;
            var sourceScope = sim.Watch(sim.getElm(0));
            var resScope = sim.Watch(sim.getElm(2));*/

            var sim = new Circuit();

            var voltage0 = sim.Create<Voltage>(Voltage.WaveType.AC);
            var diode0 = sim.Create<DiodeElm>();
            var res0 = sim.Create<Resistor>(640);
            var wire0 = sim.Create<Wire>();

            sim.Connect(voltage0, 1, diode0, 0);
            sim.Connect(diode0, 1, res0, 0);
            sim.Connect(res0, 1, wire0, 0);
            sim.Connect(wire0, 1, voltage0, 0);

            var voltScope = sim.Watch(voltage0);
            var resScope = sim.Watch(res0);

            var cycleTime = 1 / voltage0.Frequency;
            var quarterCycleTime = cycleTime / 4;

            var steps = (int)(cycleTime / sim.TimeStep);
            for (var x = 1; x <= steps; x++)
                sim.DoTick();

            // A/C Voltage Source
            {
                var voltageHigh = voltScope.Max((f) => f.Voltage);
                var voltageHighNdx = voltScope.FindIndex((f) => f.Voltage == voltageHigh);

                TestUtilities.Compare(voltageHigh, voltage0.DutyCycle, 4);
                TestUtilities.Compare(voltScope[voltageHighNdx].Time, quarterCycleTime, 4);

                var voltageLow = voltScope.Min((f) => f.Voltage);
                var voltageLowNdx = voltScope.FindIndex((f) => f.Voltage == voltageLow);

                TestUtilities.Compare(voltageLow, -voltage0.DutyCycle, 4);
                TestUtilities.Compare(voltScope[voltageLowNdx].Time, quarterCycleTime * 3, 4);
            }

            // Resistor
            {
                var voltageHigh = resScope.Max((f) => f.Voltage);
                var voltageHighNdx = resScope.FindIndex((f) => f.Voltage == voltageHigh);

                TestUtilities.Compare(resScope[voltageHighNdx].Time, quarterCycleTime, 4);

                var voltageLow = resScope.Min((f) => f.Voltage);
                var voltageLowNdx = resScope.FindIndex((f) => f.Voltage == voltageLow);

                TestUtilities.Compare(voltageLow, 0, 8);
            }

            /*string js = JsonSerializer.SerializeToString(sim);
            string nm = TestContext.CurrentContext.Test.Name;
            Debug.Log(nm);
            Debug.Log(js);
            System.IO.File.WriteAllText(string.Format("./{0}.json", nm), js);*/
        }

        [Test]
        public void FullWaveRectifierTest()
        {
            Assert.Ignore("Not Implemented!");
        }
    }
}
