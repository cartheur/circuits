using System;
using System.Linq;
using CartheurCircuit;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;
using NUnit.Framework;

namespace AnalogCircuitTests {

	[TestFixture]
	public class VoltageWaveTest {

		[Test]
		public void DcWaveTest() {
			// Flat ... is a type of wave... I guess.
			Assert.Ignore("Not Implemented!");
		}

		[Test]
		public void AcWaveTest() {
			Circuit sim = new Circuit();

			var voltage0 = sim.Create<VoltageInput>(Voltage.WaveType.AC);

			var res0 = sim.Create<Resistor>();
			var ground0 = sim.Create<Ground>();

			sim.Connect(voltage0.LeadPositive, res0.LeadInput);
			sim.Connect(res0.LeadOutput, ground0.leadIn);

			var resScope0 = sim.Watch(res0);

			double cycleTime = 1 / voltage0.Frequency;
			double quarterCycleTime = cycleTime / 4;

			int steps = (int)(cycleTime / sim.TimeStep);
			for(int x = 1; x <= steps; x++)
				sim.DoTick();

			double voltageHigh = resScope0.Max((f) => f.Voltage);
			int voltageHighNdx = resScope0.FindIndex((f) => f.Voltage == voltageHigh);

			TestUtilities.Compare(voltageHigh, voltage0.DutyCycle, 4);
			TestUtilities.Compare(resScope0[voltageHighNdx].Time, quarterCycleTime, 4);

			double voltageLow = resScope0.Min((f) => f.Voltage);
			int voltageLowNdx = resScope0.FindIndex((f) => f.Voltage == voltageLow);

			TestUtilities.Compare(voltageLow, -voltage0.DutyCycle, 4);
			TestUtilities.Compare(resScope0[voltageLowNdx].Time, quarterCycleTime * 3, 4);

			double currentHigh = resScope0.Max((f) => f.Current);
			int currentHighNdx = resScope0.FindIndex((f) => f.Current == currentHigh);
			Debug.Log(currentHigh, "currentHigh");

			double currentLow = resScope0.Min((f) => f.Current);
			int currentLowNdx = resScope0.FindIndex((f) => f.Current == currentLow);
			Debug.Log(Math.Round(currentLow, 4), "currentLow");
		}

		[Test]
		public void SquareWaveTest() {
			Circuit sim = new Circuit();

			var voltage0 = sim.Create<VoltageInput>(Voltage.WaveType.SQUARE);

			var res0 = sim.Create<Resistor>();
			var ground0 = sim.Create<Ground>();

			sim.Connect(voltage0.LeadPositive, res0.LeadInput);
			sim.Connect(res0.LeadOutput, ground0.leadIn);

			var resScope0 = sim.Watch(res0);

			double cycleTime = 1 / voltage0.Frequency;
			double quarterCycleTime = cycleTime / 4;

			int steps = (int)(cycleTime / sim.TimeStep);
			for(int x = 1; x <= steps; x++)
				sim.DoTick();

			double voltageHigh = resScope0.Max((f) => f.Voltage);
			int voltageHighNdx = resScope0.FindIndex((f) => f.Voltage == voltageHigh);

			Assert.AreEqual(voltageHigh, voltage0.DutyCycle);
			Assert.AreEqual(0, voltageHighNdx);

			double voltageLow = resScope0.Min((f) => f.Voltage);
			int voltageLowNdx = resScope0.FindIndex((f) => f.Voltage == voltageLow);

			Assert.AreEqual(voltageLow, -voltage0.DutyCycle);
			Assert.AreEqual(2501, voltageLowNdx);

			double currentHigh = resScope0.Max((f) => f.Current);
			int currentHighNdx = resScope0.FindIndex((f) => f.Current == currentHigh);
			Assert.AreEqual(voltageHigh / res0.Resistance, currentHigh);

			double currentLow = resScope0.Min((f) => f.Current);
			int currentLowNdx = resScope0.FindIndex((f) => f.Current == currentLow);
			Assert.AreEqual(voltageLow / res0.Resistance, currentLow);
		}

		[Test]
		public void TriangleWaveVoltageTest() {
			Assert.Ignore("Not Implemented!");
		}

		[Test]
		public void SawtoothWaveVoltageTest() {
			Assert.Ignore("Not Implemented!");
		}

		[Test]
		public void PulseWaveVoltageTest() {
			Assert.Ignore("Not Implemented!");
		}

		[Test]
		public void VarWaveVoltageTest() {
			Assert.Ignore("Not Implemented!");
		}

	}
}
