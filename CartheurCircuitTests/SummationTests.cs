using System;
using CartheurAnalytics;
using CartheurCircuit;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;
using NUnit.Framework;

namespace AnalogCircuitTests
{
    [TestFixture]
    public class SummationTests
    {
        [Test]
        public void SimpleSummation()
        {

        }

        [Test]
        public void FeedbackTest()
        {
            Circuit simulation = new Circuit();

            var volt0 = simulation.Create<VoltageInput>(Voltage.WaveType.DC);
            var opAmp0 = simulation.Create<OpAmp>();
            var analogOut0 = simulation.Create<Output>();

            simulation.Connect(opAmp0.OutputLead, opAmp0.NegativeLead);
            simulation.Connect(volt0.LeadPositive, opAmp0.PositiveLead);
            simulation.Connect(analogOut0.leadIn, opAmp0.OutputLead);

            for (int x = 1; x <= 100; x++)
                simulation.DoTick();

            TestUtilities.Compare(analogOut0.GetVoltageDelta(), 5, 2);
            Console.WriteLine("The analogue out is " + analogOut0.GetVoltageDelta() + "where it should be " + "5.");
        }

        [Test]
        public void SimpleIntegration()
        {
            //simpson.RelativeTolerance = 1e-5; // From the code logic-copy additions.
            //simpson.Integrate(Math.Sin, 0, 2);
            const double twopi = 2.0 * Math.PI;
            const double epsilon = 1e-8;
            var integral = SimpsonIntegrator.Integrate(x => Math.Cos(x) + 1.0, 0,
                twopi, epsilon);
            const double expected = twopi;
            Assert.IsTrue(Math.Abs(integral - expected) < epsilon);
        }
    }
}
