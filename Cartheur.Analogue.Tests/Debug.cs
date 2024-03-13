using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpCircuitTest
{
	public static class Debug
	{

		public static void Log(params object[] objs)
		{
			var sb = new StringBuilder();
			foreach (var o in objs)
				sb.Append(o).Append(" ");
			Console.WriteLine(sb.ToString());
		}

		public static void LogF(string format, params object[] objs)
		{
			Console.WriteLine(format, objs);
		}

	}
}
