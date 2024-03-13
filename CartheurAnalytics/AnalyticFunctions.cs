using System;

namespace CartheurAnalytics
{
    public class AnalyticFunctions
    {
        /// <summary>
        /// Function of x * y.
        /// </summary>
        /// <param name="x">The value of x.</param>
        /// <param name="y">The value of y.</param>
        public static double P(double x, double y)
        {
            return x * y;
        }
        /// <summary>
        /// Function of x^2 * y^3.
        /// </summary>
        /// <param name="x">The value of x.</param>
        /// <param name="y">The value of y.</param>
        public static double Q(double x, double y)
        {
            return x * x * y * y * y;
        }
        /// <summary>
        /// Function of 5.
        /// </summary>
        /// <param name="x">The value of x.</param>
        /// <param name="y">The value of y.</param>
        public static double R(double x, double y)
        {
            return 5;
        }
        /// <summary>
        /// Function representing a two-dimensional hemisphere object.
        /// </summary>
        /// <param name="x">The value of x.</param>
        /// <param name="y">The value of y.</param>
        /// <param name="radius">The radius value.</param>
        public static double HemiSphere2D(double x, double y, double radius)
        {
            return Math.Sqrt((2 * radius) - x * x - y * y);
        }
        /// <summary>
        /// Function representing a three-dimensional hemispherical object.
        /// </summary>
        /// <param name="x">The value of x.</param>
        /// <param name="y">The value of y.</param>
        /// <param name="z">The value of z.</param>
        /// <param name="radius">The radius value.</param>
        public static double HemiSphere3D(double x, double y, double z, double radius)
        {
            return Math.Sqrt((2 * radius) - x * x - y * y - z * z);
        }

        public static double One(double x)
        {
            return 1;
        }

        public static double MinusOne(double x)
        {
            return -1;
        }
        /// <summary>
        /// Function of x.
        /// </summary>
        /// <param name="x">The value of x.</param>
        public static double F1(double x)
        {
            return x;
        }
        /// <summary>
        /// Function of 2 * x.
        /// </summary>
        /// <param name="x">The value of x.</param>
        public static double F2(double x)
        {
            return 2 * x;
        }
        /// <summary>
        /// Function of sine.
        /// </summary>
        /// <param name="x">The value of x.</param>
        public static double Sine(double x)
        {
            return Math.Sin(x);
        }
        /// <summary>
        /// Function representing a semicircle object.
        /// </summary>
        /// <param name="x">The value of x.</param>
        public static double SemiCircle(double x)
        {
            return Math.Sqrt(1 - x * x);
        }

    }
}
