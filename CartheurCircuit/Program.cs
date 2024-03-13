using System;
using System.Text;
using CartheurCircuit.Elements;
using CartheurCircuit.Elements.Sources;

namespace CartheurCircuit {
	
	class Program {

		public static double Round(double val, int places) {
			if(places < 0) throw new ArgumentException("places");
			return Math.Round(val - (0.5 / Math.Pow(10, places)), places);
		}

		static void Main(string[] args) {

			Circuit sim = new Circuit();

			var volt0 = sim.Create<VoltageInput>(Voltage.WaveType.DC);
			var res0 = sim.Create<Resistor>();
			var ground0 = sim.Create<Ground>();

			sim.Connect(volt0.LeadPositive, res0.LeadInput);
			sim.Connect(res0.LeadOutput, ground0.leadIn);

			for(int x = 1; x <= 100; x++) {
				sim.DoTick();
				// Ohm's Law
				Debug.Log(res0.GetVoltageDelta(), res0.Resistance * res0.GetCurrent()); // V = I x R
				Debug.Log(res0.GetCurrent(), res0.GetVoltageDelta() / res0.Resistance); // I = V / R
				Debug.Log(res0.Resistance, res0.GetVoltageDelta() / res0.GetCurrent()); // R = V / I
			}

			Console.WriteLine("program complete");
			Console.ReadLine();
		}

	}
}

public static class Debug {

	public static void Log(params object[] objs) {
		var sb = new StringBuilder();
		foreach(var o in objs)
			sb.Append(o).Append(" ");
		Console.WriteLine(sb.ToString());
	}

	public static void LogF(string format, params object[] objs) {
		Console.WriteLine(format, objs);
	}

}