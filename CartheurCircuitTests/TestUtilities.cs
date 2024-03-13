using System;
using NUnit.Framework;

namespace AnalogCircuitTests
{
    public static class TestUtilities
    {
        public const double TestEpsilon = 1E-6;
        /// <summary>
        /// Compares the specified inputs.
        /// </summary>
        /// <param name="a">First input to compare.</param>
        /// <param name="b">Second input to compare.</param>
        /// <param name="tolerance">The specified tolerance.</param>
        public static void Compare(double a, double b, int tolerance)
        {
            Func<double, int, double> round = (val, places) => Math.Round(val - (0.5 / Math.Pow(10, places)), places);
            Assert.That(round(a, tolerance), Is.EqualTo(round(b, tolerance)).Within(Math.Pow(10, -tolerance)));
        }
    }
}
