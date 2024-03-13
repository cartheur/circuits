using System;
using CartheurCircuit;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;
using NUnit.Framework;

namespace AnalogCircuitTests
{
    [TestFixture]
    public class SimpleCircuitTest
    {
        //Assert.That(1.0, Is.EqualTo(1.5).Within(0.5f));
        [TestCase(2.0, 0.5)]
        public void MyCircuitTest(double voltage, double current)
        {
            var simulation = new Circuit();
            var source = simulation.Create<CurrentSource>(current);
            var potential = simulation.Create<VoltageInput>(voltage);
            source.SourceCurrent = current;
            potential.SetCurrent(Convert.ToInt32(potential), current);
            var resistor = simulation.Create<Resistor>();
        }

        [Test]
        public void AnalogSwitchTest()
        {
            var sim = new Circuit();
            var logicIn0 = sim.Create<LogicInput>();
            var logicIn1 = sim.Create<LogicInput>();
            var analogSwitch0 = sim.Create<AnalogSwitch>();
            var grnd = sim.Create<Ground>();

            sim.Connect(logicIn0.LeadOutput, analogSwitch0.LeadIn);
            sim.Connect(logicIn1.LeadOutput, analogSwitch0.LeadSwitch);
            sim.Connect(analogSwitch0.LeadOut, grnd.leadIn);

            logicIn0.Toggle();
            logicIn1.Toggle();

            sim.DoTicks(100);
            Assert.AreEqual(0.25, Math.Round(grnd.GetCurrent(), 12));

            logicIn1.Toggle();
            sim.Analyze();
            sim.DoTicks(100);
            Assert.AreEqual(5E-10, Math.Round(grnd.GetCurrent(), 12));
        }

        [TestCase(0.01)]
        [TestCase(0.02)]
        [TestCase(0.04)]
        [TestCase(0.06)]
        [TestCase(0.08)]
        [TestCase(0.10)]
        public void CurrentSourceTest(double current)
        {
            var simulation = new Circuit();

            var source = simulation.Create<CurrentSource>(current);
            source.SourceCurrent = current;
            var resistor = simulation.Create<Resistor>();

            simulation.Connect(source.LeadOutput, resistor.LeadInput);
            simulation.Connect(resistor.LeadOutput, source.LeadInput);

            var source1 = simulation.Create<CurrentSource>();
            source1.SourceCurrent = current;
            var resistor1 = simulation.Create<Resistor>();

            simulation.Connect(source1.LeadOutput, resistor1.LeadOutput);
            simulation.Connect(resistor1.LeadInput, source1.LeadInput);

            simulation.DoTicks(100);

            Assert.AreEqual(current, resistor.GetCurrent(), 1E-3);
            Assert.AreEqual(-current, resistor1.GetCurrent());
            Assert.AreEqual(current * resistor.Resistance, resistor.GetVoltageDelta(), TestUtilities.TestEpsilon);
        }

        [TestCase(20)]
        [TestCase(40)]
        [TestCase(60)]
        [TestCase(80)]
        [TestCase(100)]
        [TestCase(120)]
        [TestCase(140)]
        [TestCase(160)]
        [TestCase(180)]
        public void ResistorTest(double resistance)
        {
            var simulation = new Circuit();

            var voltage = simulation.Create<DcVoltageSource>();
            var resistor = simulation.Create<Resistor>(resistance);

            simulation.Connect(voltage.LeadPositive, resistor.LeadOutput);
            simulation.Connect(resistor.LeadInput, voltage.LeadNegative);

            for (var x = 1; x <= 100; x++)
            {
                simulation.DoTick();
                // Ohm's Law
                Assert.AreEqual(resistor.GetVoltageDelta(), resistor.Resistance * resistor.GetCurrent()); // V = I x R
                Assert.AreEqual(resistor.GetCurrent(), resistor.GetVoltageDelta() / resistor.Resistance); // I = V / R
                Assert.AreEqual(resistor.Resistance, resistor.GetVoltageDelta() / resistor.GetCurrent()); // R = V / I
            }
        }

        [TestCase(20)]
        [TestCase(40)]
        [TestCase(60)]
        [TestCase(80)]
        [TestCase(100)]
        [TestCase(120)]
        [TestCase(140)]
        [TestCase(160)]
        [TestCase(180)]
        public void LinearResistorTest(double resistance)
        {
            var sim = new Circuit();

            var volt0 = sim.Create<VoltageInput>(Voltage.WaveType.DC);
            var res0 = sim.Create<Resistor>();
            var ground0 = sim.Create<Ground>();

            sim.Connect(volt0.LeadPositive, res0.LeadInput);
            sim.Connect(res0.LeadOutput, ground0.leadIn);

            for (var x = 1; x <= 100; x++)
            {
                sim.DoTick();
                // Ohm's Law
                Assert.AreEqual(res0.GetVoltageDelta(), res0.Resistance * res0.GetCurrent()); // V = I x R
                Assert.AreEqual(res0.GetCurrent(), res0.GetVoltageDelta() / res0.Resistance); // I = V / R
                Assert.AreEqual(res0.Resistance, res0.GetVoltageDelta() / res0.GetCurrent()); // R = V / I
            }
        }

        [TestCase(1)]
        [TestCase(0.02)]
        [TestCase(0.4)]
        public void InductorTest(double inductance)
        {
            var sim = new Circuit();

            var source = sim.Create<DcVoltageSource>();
            var inductor = sim.Create<InductorElement>(inductance);

            sim.Connect(source.LeadPositive, inductor.LeadInput);
            sim.Connect(inductor.LeadOutput, source.LeadNegative);

            var cycleTime = 1 / source.Frequency;
            var quarterCycleTime = cycleTime / 4;

            sim.DoTicks((int)(cycleTime / sim.TimeStep));

            var flux = inductor.Inductance * inductor.GetCurrent();	// F = I x L
            Assert.AreEqual(inductor.GetCurrent(), flux / inductor.Inductance); // I = F / L
            Assert.AreEqual(inductor.Inductance, flux / inductor.GetCurrent()); // L = F / I
        }

        [TestCase(1)]
        [TestCase(0.02)]
        [TestCase(0.4)]
        public void LinearInductorTest(double inductance)
        {
            var sim = new Circuit();

            var source0 = sim.Create<VoltageInput>(Voltage.WaveType.AC);
            var inductor0 = sim.Create<InductorElement>(inductance);
            var out0 = sim.Create<Ground>();

            sim.Connect(source0.LeadPositive, inductor0.LeadInput);
            sim.Connect(inductor0.LeadOutput, out0.leadIn);

            var cycleTime = 1 / source0.Frequency;
            var quarterCycleTime = cycleTime / 4;

            sim.DoTicks((int)(cycleTime / sim.TimeStep));

            Debug.Log(inductor0.GetCurrent());
            Assert.Ignore();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SwitchSpstTest(bool input)
        {
            var simulation = new Circuit();

            var voltage = simulation.Create<VoltageInput>();
            var switch0 = simulation.Create<SwitchSpst>();
            var resistor = simulation.Create<Resistor>();
            var logicOut = simulation.Create<LogicOutput>();

            simulation.Connect(voltage, 0, switch0, 0);
            simulation.Connect(switch0, 1, resistor, 0);
            simulation.Connect(resistor, 1, logicOut, 0);

            var volt1 = simulation.Create<VoltageInput>();
            var switch1 = simulation.Create<SwitchSpst>();
            var res1 = simulation.Create<Resistor>();
            var grnd1 = simulation.Create<Ground>();

            simulation.Connect(volt1, 0, switch1, 0);
            simulation.Connect(switch1, 1, res1, 0);
            simulation.Connect(res1, 1, grnd1, 0);

            if (input)
            {
                switch0.Toggle();
                switch1.Toggle();
            }

            simulation.DoTicks(100);

            Debug.Log(logicOut.GetVoltageDelta(), logicOut.GetCurrent());
            Debug.Log(grnd1.GetVoltageDelta(), grnd1.GetCurrent());
            Assert.Ignore();
        }

        [TestCase(1, false)]
        [TestCase(0, true)]
        public void InverterTest(int in0, bool out0)
        {
            var sim = new Circuit();

            var logicIn0 = sim.Create<LogicInput>();
            var invert0 = sim.Create<Inverter>();
            var logicOut = sim.Create<LogicOutput>();

            sim.Connect(logicIn0.LeadOutput, invert0.leadIn);
            sim.Connect(invert0.leadOut, logicOut.leadIn);

            logicIn0.SetPosition(in0);
            sim.Analyze();

            sim.DoTicks(100);

            Assert.AreEqual(out0, logicOut.isHigh());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void LogicInputOutputTest(bool input) {
            var sim = new Circuit();

            var logicInput = sim.Create<LogicInput>();
            var logicOutput = sim.Create<LogicOutput>();

            Assert.Ignore();
        }

        [TestCase(0.005)]
        [TestCase(0.200)]
        [TestCase(0.400)]
        [TestCase(0.5)]
        [TestCase(0.600)]
        [TestCase(0.800)]
        [TestCase(0.995)]
        public void PotentiometerTest(double inputPosition)
        {
            var simulation = new Circuit();

            var voltage = simulation.Create<LogicInput>();
            voltage.Toggle();

            var potentiometer = simulation.Create<Potentiometer>(100);
            potentiometer.Position = inputPosition; // 0.995

            var groundZero = simulation.Create<Ground>();
            var groundOne = simulation.Create<Ground>();

            simulation.Connect(potentiometer.LeadVoltage, voltage.LeadOutput);
            simulation.Connect(potentiometer.LeadInput, groundZero.leadIn);
            simulation.Connect(potentiometer.LeadOutput, groundOne.leadIn);

            simulation.DoTicks(100);

            Logging.Debug("groundZero", groundZero.GetCurrent());
            Logging.Debug("groundOne", groundOne.GetCurrent());
            Logging.Debug(groundZero.GetCurrent() + groundOne.GetCurrent());
            //Assert.Ignore();
        }
        [Test]
        public void SiliconRectifierTest()
        {
            var sim = new Circuit();

            var volt0 = sim.Create<VoltageInput>();
            var volt1 = sim.Create<VoltageInput>();

            var res0 = sim.Create<Resistor>();

            var scr0 = sim.Create<SiliconRectifier>();

            var grnd0 = sim.Create<Ground>();

            sim.Connect(volt0.LeadPositive, scr0.LeadIn);

            sim.Connect(volt1.LeadPositive, res0.LeadInput);
            sim.Connect(res0.LeadOutput, scr0.LeadGate);
            sim.Connect(scr0.LeadOut, grnd0.leadIn);

            sim.DoTicks(1000);

            Debug.Log(sim.Time);
            Debug.Log("in", scr0.GetLeadVoltage(0));
            Debug.Log("out", scr0.GetLeadVoltage(1));
            Debug.Log("gate", scr0.GetLeadVoltage(2));

            Debug.Log();
            foreach (var s in scr0.GetInfo())
            {
                Debug.Log(s);
            }

            Assert.Ignore();
        }
        [Test]
        public void TriodeTest()
        {
            var sim = new Circuit();

            var volt0 = sim.Create<VoltageInput>();
            volt0.MaxVoltage = 500;
            var triode0 = sim.Create<Triode>();
            var grnd0 = sim.Create<Ground>();

            sim.Connect(volt0.LeadPositive, triode0.leadPlate);
            sim.Connect(triode0.leadCath, grnd0.leadIn);

            sim.DoTicks(100);

            Assert.AreEqual(0.018332499042, Math.Round(volt0.GetCurrent(), 12));
        }

        [TestCase(0, 0, false)]
        [TestCase(1, 0, false)]
        [TestCase(0, 1, false)]
        [TestCase(1, 1, true)]
        public void TriStateBufferTest(int in0, int in1, bool in3)
        {
            var sim = new Circuit();

            var logicIn0 = sim.Create<LogicInput>();
            var logicIn1 = sim.Create<LogicInput>();
            var logicOut0 = sim.Create<LogicOutput>();
            var tri0 = sim.Create<TriStateBuffer>();

            sim.Connect(logicIn0.LeadOutput, tri0.leadIn);
            sim.Connect(logicIn1.LeadOutput, tri0.leadGate);
            sim.Connect(logicOut0.leadIn, tri0.leadOut);

            logicIn0.SetPosition(in0);
            logicIn1.SetPosition(in1);

            sim.DoTicks(100);

            Assert.AreEqual(in3, logicOut0.isHigh());
        }

    }
}
