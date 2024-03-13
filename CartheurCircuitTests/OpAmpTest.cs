using System;
using CartheurCircuit;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;
using NUnit.Framework;

namespace AnalogCircuitTests
{
    [TestFixture]
    public class OpAmpTest
    {
        [Test]
        public void SimpleOpAmpTest()
        {
            var simulation = new Circuit();

            var volt0 = simulation.Create<VoltageInput>(Voltage.WaveType.DC);
            volt0.MaxVoltage = 3;

            var volt1 = simulation.Create<VoltageInput>(Voltage.WaveType.DC);
            volt1.MaxVoltage = 4;

            var opAmp0 = simulation.Create<OpAmp>();
            var analogOut0 = simulation.Create<Output>();

            simulation.Connect(volt0.LeadPositive, opAmp0.NegativeLead);
            simulation.Connect(volt1.LeadPositive, opAmp0.PositiveLead);
            simulation.Connect(opAmp0.OutputLead, analogOut0.leadIn);

            for (int x = 1; x <= 100; x++)
                simulation.DoTick();

            TestUtilities.Compare(analogOut0.GetVoltageDelta(), 15, 2);
        }
        [Test]
        public void OpAmpFeedbackTest()
        {
            Circuit sim = new Circuit();

            var volt0 = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            var opAmp0 = sim.Create<OpAmp>();
            var analogOut0 = sim.Create<Output>();

            sim.Connect(opAmp0.OutputLead, opAmp0.NegativeLead);
            sim.Connect(volt0.LeadPositive, opAmp0.PositiveLead);
            sim.Connect(analogOut0.leadIn, opAmp0.OutputLead);

            for (int x = 1; x <= 100; x++)
                sim.DoTick();

            TestUtilities.Compare(analogOut0.GetVoltageDelta(), 5, 2);
        }
        [Test]
        public void GyratorTest()
        {
            Circuit sim = new Circuit();

            var square0 = sim.Create<VoltageInput>(Voltage.WaveType.SQUARE);
            square0.Frequency = 20;

            var res0 = sim.Create<Resistor>(1000);
            var res1 = sim.Create<Resistor>(20000);
            var cap0 = sim.Create<Capacitor>(2.5E-7);
            var opAmp0 = sim.Create<OpAmp>();
            var grnd0 = sim.Create<Ground>();

            sim.Connect(square0.LeadPositive, res0.LeadInput);
            sim.Connect(opAmp0.NegativeLead, res0.LeadOutput);
            sim.Connect(opAmp0.OutputLead, opAmp0.NegativeLead);

            sim.Connect(square0.LeadPositive, cap0.LeadIn);
            sim.Connect(opAmp0.PositiveLead, cap0.LeadOut);
            sim.Connect(cap0.LeadOut, res1.LeadInput);
            sim.Connect(res1.LeadOutput, grnd0.leadIn);

            var square1 = sim.Create<VoltageInput>(Voltage.WaveType.SQUARE);
            square1.Frequency = 20;

            var res3 = sim.Create<Resistor>(1000);
            var induct0 = sim.Create<InductorElement>(5);
            var grnd1 = sim.Create<Ground>();

            sim.Connect(square1.LeadPositive, res3.LeadInput);
            sim.Connect(res3.LeadOutput, induct0.LeadInput);
            sim.Connect(induct0.LeadOutput, grnd0.leadIn);

            var scope0 = sim.Watch(square0);
            var scope1 = sim.Watch(square1);

            double cycleTime = 1 / square0.Frequency;
            double quarterCycleTime = cycleTime / 4;
            int steps = (int)(cycleTime / sim.TimeStep);
            sim.DoTicks(steps);

            for (int x = 0; x < scope0.Count; x++)
            {
                Assert.AreEqual(scope0[x].Voltage, scope1[x].Voltage);
                Assert.AreEqual(0, Math.Round(scope0[x].Current - scope1[x].Current, 3));
            }

        }
        [Test]
        public void CapacitanceMultiplierTest()
        {
            Circuit sim = new Circuit();

            var square0 = sim.Create<VoltageInput>(Voltage.WaveType.SQUARE);
            square0.Frequency = 30;

            var res0 = sim.Create<Resistor>(100000);
            var res1 = sim.Create<Resistor>(1000);
            var cap0 = sim.Create<Capacitor>(1E-7);
            var opAmp0 = sim.Create<OpAmp>();
            var grnd0 = sim.Create<Ground>();

            sim.Connect(square0.LeadPositive, res0.LeadInput);
            sim.Connect(square0.LeadPositive, res1.LeadInput);

            sim.Connect(opAmp0.NegativeLead, res1.LeadOutput);
            sim.Connect(opAmp0.NegativeLead, opAmp0.OutputLead);

            sim.Connect(opAmp0.PositiveLead, res0.LeadOutput);
            sim.Connect(opAmp0.PositiveLead, cap0.LeadIn);

            sim.Connect(cap0.LeadOut, grnd0.leadIn);

            var square1 = sim.Create<VoltageInput>(Voltage.WaveType.SQUARE);
            square1.Frequency = 30;

            var cap1 = sim.Create<Capacitor>(1E-5);
            var res2 = sim.Create<Resistor>(1000);
            var grnd1 = sim.Create<Ground>();

            sim.Connect(square1.LeadPositive, cap1.LeadIn);
            sim.Connect(cap1.LeadOut, res2.LeadInput);
            sim.Connect(res2.LeadOutput, grnd1.leadIn);

            var scope0 = sim.Watch(cap0);
            var scope1 = sim.Watch(cap1);

            double cycleTime = 1 / square0.Frequency;
            double quarterCycleTime = cycleTime / 4;
            int steps = (int)(cycleTime / sim.TimeStep);
            sim.DoTicks(steps);

            /*Assert.AreEqual( 4.06, Math.Round(scope0.Max((f) => f.voltage), 2));
            Assert.AreEqual(-3.29, Math.Round(scope0.Min((f) => f.voltage), 2));
            Assert.AreEqual( 4.06, Math.Round(scope1.Max((f) => f.voltage), 2));
            Assert.AreEqual(-3.29, Math.Round(scope1.Min((f) => f.voltage), 2));

            Assert.AreEqual( 0.004999, Math.Round(scope0.Max((f) => f.current) * 100, 6));
            Assert.AreEqual(-0.009054, Math.Round(scope0.Min((f) => f.current) * 100, 6));
            Assert.AreEqual( 0.004999, Math.Round(scope1.Max((f) => f.current), 6));
            Assert.AreEqual(-0.009054, Math.Round(scope1.Min((f) => f.current), 6));*/

            for (int x = 0; x < scope0.Count; x++)
            {
                Assert.AreEqual(Math.Round(scope0[x].Voltage, 6), Math.Round(scope1[x].Voltage, 6));
                Assert.AreEqual(Math.Round(scope0[x].Current * 100, 6), Math.Round(scope1[x].Current, 6));
            }

            scope0.Clear();
            scope1.Clear();
            sim.DoTicks(steps);

            /*Assert.AreEqual( 3.43, Math.Round(scope0.Max((f) => f.voltage), 2));
            Assert.AreEqual(-3.41, Math.Round(scope0.Min((f) => f.voltage), 2));
            Assert.AreEqual( 3.43, Math.Round(scope1.Max((f) => f.voltage), 2));
            Assert.AreEqual(-3.41, Math.Round(scope1.Min((f) => f.voltage), 2));

            Assert.AreEqual( 0.008287, Math.Round(scope0.Max((f) => f.current) * 100, 6));
            Assert.AreEqual(-0.008432, Math.Round(scope0.Min((f) => f.current) * 100, 6));
            Assert.AreEqual( 0.008287, Math.Round(scope1.Max((f) => f.current), 6));
            Assert.AreEqual(-0.008432, Math.Round(scope1.Min((f) => f.current), 6));*/

            for (int x = 0; x < scope0.Count; x++)
            {
                Assert.AreEqual(Math.Round(scope0[x].Voltage, 6), Math.Round(scope1[x].Voltage, 6));
                Assert.AreEqual(Math.Round(scope0[x].Current * 100, 6), Math.Round(scope1[x].Current, 6));
            }

        }

    }
}
