using System.Linq;
using CartheurCircuit;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;
using NUnit.Framework;

namespace AnalogCircuitTests
{
    [TestFixture]
    public class DcVoltageTest
    {
        [Test]
        public void CapacitorTest()
        {
            var sim = new Circuit();

            var volt0 = sim.Create<DcVoltageSource>();
            var cap0 = sim.Create<Capacitor>(2E-4);
            var res0 = sim.Create<Resistor>();

            var switch0 = sim.Create<SwitchSpdt>();

            /*sim.Connect(volt0, 1, switch0, 1);
            sim.Connect(switch0, 1, switch0, 1);
            sim.Connect(switch0, 2, res0, 1);
            sim.Connect(cap0, 1, res0, 0);
            sim.Connect(res0, 1, volt0, 0);*/

            // leadNeg 0
            // leadPos 1

            sim.Connect(volt0, 1, switch0, 1);
            sim.Connect(switch0, 0, cap0, 0);
            sim.Connect(cap0, 1, res0, 0);
            sim.Connect(res0, 1, volt0, 0);
            sim.Connect(switch0, 2, volt0, 0);

            switch0.SetPosition(0);

            var capScope0 = sim.Watch(cap0);

            for (var x = 1; x <= 28000; x++)
                sim.DoTick();

            Debug.LogF("{0} [{1}]", sim.Time, SiUnits.Normalize(sim.Time, "s"));
            {
                var voltageHigh = capScope0.Max((f) => f.Voltage);
                var voltageHighNdx = capScope0.FindIndex((f) => f.Voltage == voltageHigh);

                Debug.Log("voltageHigh", voltageHigh, voltageHighNdx);

                var voltageLow = capScope0.Min((f) => f.Voltage);
                var voltageLowNdx = capScope0.FindIndex((f) => f.Voltage == voltageLow);

                Debug.Log("voltageLow ", voltageLow, voltageLowNdx);

                var currentHigh = capScope0.Max((f) => f.Current);
                var currentHighNdx = capScope0.FindIndex((f) => f.Current == currentHigh);
                Debug.Log("currentHigh", currentHigh, currentHighNdx);

                var currentLow = capScope0.Min((f) => f.Current);
                var currentLowNdx = capScope0.FindIndex((f) => f.Current == currentLow);

                Debug.Log("currentLow ", currentLow, currentLowNdx);

                Assert.AreEqual(27999, voltageHighNdx);
                Assert.AreEqual(0, voltageLowNdx);

                Assert.AreEqual(0, currentHighNdx);
                Assert.AreEqual(27999, currentLowNdx);
            }

            switch0.SetPosition(1);
            sim.Analyze();
            capScope0.Clear();

            for (var x = 1; x <= 28000; x++)
                sim.DoTick();

            Debug.Log();

            Debug.LogF("{0} [{1}]", sim.Time, SiUnits.Normalize(sim.Time, "s"));
            {
                var voltageHigh = capScope0.Max((f) => f.Voltage);
                var voltageHighNdx = capScope0.FindIndex((f) => f.Voltage == voltageHigh);

                Debug.Log("voltageHigh ", voltageHigh, voltageHighNdx);

                var voltageLow = capScope0.Min((f) => f.Voltage);
                var voltageLowNdx = capScope0.FindIndex((f) => f.Voltage == voltageLow);

                Debug.Log("voltageLow  ", voltageLow, voltageLowNdx);

                var currentHigh = capScope0.Max((f) => f.Current);
                var currentHighNdx = capScope0.FindIndex((f) => f.Current == currentHigh);
                Debug.Log("currentHigh", currentHigh, currentHighNdx);

                var currentLow = capScope0.Min((f) => f.Current);
                var currentLowNdx = capScope0.FindIndex((f) => f.Current == currentLow);

                Debug.Log("currentLow ", currentLow, currentLowNdx);

                Assert.AreEqual(voltageHighNdx, currentLowNdx);
                Assert.AreEqual(voltageLowNdx, currentHighNdx);

                Assert.AreEqual(0, voltageHighNdx);
                Assert.AreEqual(27999, voltageLowNdx);

                Assert.AreEqual(27999, currentHighNdx);
                Assert.AreEqual(0, currentLowNdx);
            }
        }
    }
}
