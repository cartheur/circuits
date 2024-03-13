using System;
using System.Collections.Generic;
using System.Linq;
using CartheurCircuit;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;
using NUnit.Framework;

namespace AnalogCircuitTests
{
    [TestFixture]
    public class ResistorTest
    {
        [Test]
        public void LeastResistanceTest()
        {
            var sim = new Circuit();

            var volt0 = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            var res0 = sim.Create<Resistor>(100);
            var res1 = sim.Create<Resistor>(1000);
            var ground0 = sim.Create<Ground>();
            var ground1 = sim.Create<Ground>();

            sim.Connect(volt0, 0, res0, 0);
            sim.Connect(volt0, 0, res1, 0);
            sim.Connect(res0, 1, ground0, 0);
            sim.Connect(res1, 1, ground1, 0);

            for (int x = 1; x <= 100; x++)
                sim.DoTick();

            TestUtilities.Compare(ground0.GetCurrent(), 0.05, 8);
            TestUtilities.Compare(ground1.GetCurrent(), 0.005, 8);
        }

        [TestCase(2)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        [TestCase(10)]
        public void LawOfResistorsInSeriesTest(int in0)
        {
            var sim = new Circuit();

            var volt0 = sim.Create<DcVoltageSource>();
            var volt1 = sim.Create<DcVoltageSource>();
            var resCompare = sim.Create<Resistor>(in0 * 100);

            var resistors = new List<Resistor>();
            for (int i = 0; i < in0; i++)
                resistors.Add(sim.Create<Resistor>());

            sim.Connect(volt0.LeadPositive, resistors.First().LeadInput);

            for (int i = 1; i < in0 - 1; i++)
                sim.Connect(resistors[i - 1].LeadOutput, resistors[i].LeadInput);

            sim.Connect(volt0.LeadNegative, resistors.Last().LeadOutput);

            sim.Connect(volt1.LeadPositive, resCompare.LeadInput);
            sim.Connect(resCompare.LeadOutput, volt1.LeadNegative);

            sim.DoTicks(100);

            Assert.AreEqual(Math.Round(resistors.Last().GetCurrent(), 12), Math.Round(resCompare.GetCurrent(), 12));
        }
        [Test]
        public void VoltageDividerTest()
        {
            var sim = new Circuit();

            var voltage = sim.Create<DcVoltageSource>();
            voltage.MaxVoltage = 10;

            var res0 = sim.Create<Resistor>(10000);
            var res1 = sim.Create<Resistor>(10000);

            sim.Connect(voltage, 1, res0, 0);
            sim.Connect(res0, 1, res1, 0);
            sim.Connect(res1, 1, voltage, 0);

            var res2 = sim.Create<Resistor>(10000);
            var res3 = sim.Create<Resistor>(10000);
            var res4 = sim.Create<Resistor>(10000);
            var res5 = sim.Create<Resistor>(10000);

            sim.Connect(voltage, 1, res2, 0);
            sim.Connect(res2, 1, res3, 0);
            sim.Connect(res3, 1, res4, 0);
            sim.Connect(res4, 1, res5, 0);
            sim.Connect(res5, 1, voltage, 0);

            var out0 = sim.Create<Output>();
            var out1 = sim.Create<Output>();
            var out2 = sim.Create<Output>();
            var out3 = sim.Create<Output>();

            sim.Connect(out0, 0, res0, 1);
            sim.Connect(out1, 0, res2, 1);
            sim.Connect(out2, 0, res3, 1);
            sim.Connect(out3, 0, res4, 1);

            for (int x = 1; x <= 100; x++)
                sim.DoTick();

            TestUtilities.Compare(res0.GetVoltageDelta(), 5.0, 8);
            TestUtilities.Compare(res1.GetVoltageDelta(), 5.0, 8);
            TestUtilities.Compare(res2.GetVoltageDelta(), 2.5, 8);
            TestUtilities.Compare(res3.GetVoltageDelta(), 2.5, 8);
            TestUtilities.Compare(res4.GetVoltageDelta(), 2.5, 8);
            TestUtilities.Compare(res5.GetVoltageDelta(), 2.5, 8);

            TestUtilities.Compare(out0.GetVoltageDelta(), 5.0, 8);
            TestUtilities.Compare(out1.GetVoltageDelta(), 7.5, 8);
            TestUtilities.Compare(out2.GetVoltageDelta(), 5.0, 8);
            TestUtilities.Compare(out3.GetVoltageDelta(), 2.5, 8);
        }
        [Test]
        public void WheatstoneBridgeTest()
        {
            var sim = new Circuit();

            var volt0 = sim.Create<DcVoltageSource>();

            var res0 = sim.Create<Resistor>(200);
            var res1 = sim.Create<Resistor>(400);

            sim.Connect(volt0, 1, res0, 0);
            sim.Connect(volt0, 1, res1, 0);

            var wire0 = sim.Create<Wire>();

            sim.Connect(wire0, 0, res0, 1);
            sim.Connect(wire0, 1, res1, 1);

            var res2 = sim.Create<Resistor>(100);
            var resX = sim.Create<Resistor>(200);

            sim.Connect(res0, 1, res2, 0);
            sim.Connect(res1, 1, resX, 0);

            sim.Connect(volt0, 0, res2, 1);
            sim.Connect(volt0, 0, resX, 1);

            for (int x = 1; x <= 100; x++)
                sim.DoTick();

            TestUtilities.Compare(0.025, volt0.GetCurrent(), 3);

            TestUtilities.Compare(res0.GetCurrent(), 0.01666667, 8);
            TestUtilities.Compare(res1.GetCurrent(), 0.00833334, 8);
            TestUtilities.Compare(res2.GetCurrent(), 0.01666667, 8);
            TestUtilities.Compare(resX.GetCurrent(), 0.00833334, 8);

            Assert.AreEqual(0, wire0.GetCurrent());
        }

    }
}
