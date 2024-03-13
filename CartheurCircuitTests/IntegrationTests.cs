using System;
using CartheurAnalytics;
using NUnit.Framework;

namespace AnalogCircuitTests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void SimpleSimpsonMethod()
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
        [Test]
        public void ComplexSimpsonMethod()
        {
            const double a = 2;
            const double b = 5;
            const double epsilon = 1e-10;
            int functionEvalsUsed;
            double estimatedError;
            const int maxStages = 1;

            var integral = SimpsonIntegrator.Integrate(x => x * x * x, a, b, epsilon,
                                                          maxStages, out functionEvalsUsed, out estimatedError);
            const double expected = (b * b * b * b - a * a * a * a) / 4.0;
            Assert.AreEqual(functionEvalsUsed, 3);
            Assert.IsTrue(Math.Abs(integral - expected) < epsilon);
        }
    }
    [TestFixture]
    public class LambdaNumeric
    {
        #region Test functions
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

        #endregion

        /// <summary>
        /// Delta provides the intrinsic accuracy of the computational method.
        /// </summary>
        private const double Delta = 1E-7;

        [Test]
        public void LambdaSimpsonIntegration()
        {
            Integral integrate1 = LambdaCalculus.BasicSimpsonIntegration();
            var result = integrate1(Sine, 0, Math.PI, 60);
            const double expected = 2.00000008353986;
            Assert.AreEqual(expected, result, Delta);
        }
        /// <summary>
        /// Analytically, exactly 2 for the integrals - note the superior convergence of Gauss 4-point (Check Logic).
        /// </summary>
        [Test]
        public void LambdaGaussianIntegration()
        {
            Integral integrate2 = LambdaCalculus.GaussianRuleIntetration();
            var results = integrate2(Sine, 0, Math.PI, 15);
            const int expects = 2;
            Assert.AreEqual(expects, results, Delta);

            var nextResult = integrate2(Math.Sin, 0, Math.PI, 15);
            // Since this method checks the exactness of its cousin, delta should be zero.
            Assert.AreEqual(results, nextResult);
        }
        //[Test]
        //public void LambdaDoubleIntegration()
        //{
        //    var integrate1 = LambdaCalculus.BasicSimpsonIntegration();
        //    var integrate2 = LambdaCalculus.GaussianRuleIntetration();
        //    var result = LambdaCalculus.DoubleIntegration(P, F1, F2, 1, 2, 4, 4, integrate1, integrate2);
        //    // result 5.625, analytically 5 5/8.
        //    var frac = new Fraction(45, 8);
        //    double analyticResult = frac;
        //    Assert.AreEqual(result, analyticResult, Delta);
        //}
        [Test]
        public void LambdaSurfaceArea()
        {
            var dCenter = LambdaCalculus.D5PointCenter()(Math.Sin, Math.PI);
            var dForward = LambdaCalculus.D5PointForward()(Math.Sin, Math.PI);
            var ddCenter = LambdaCalculus.DdPointCenter()(Math.Cos, Math.PI);

            var length1 = LambdaCalculus.CurveLength(F1, 0, Math.Sqrt(2), LambdaCalculus.GaussianRuleIntetration(), 2, LambdaCalculus.D5PointCenter());
            var length2 = 2 * LambdaCalculus.CurveLength(SemiCircle, -Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, LambdaCalculus.GaussianRuleIntetration(), 10, LambdaCalculus.D5PointCenter());
            var pDiffx = LambdaCalculus.PartialDifferentiationx(Q, 5, 3);
            var pDiffy = LambdaCalculus.PartialDifferentiationy(Q, 5, 3);

            var area1 = LambdaCalculus.SurfaceArea2D(P, F1, F2, 1, 4, 4, 4);
            var area2 = LambdaCalculus.SurfaceArea2D(R, F1, F2, 1, 4, 4, 4);
            const double expected = 7.5;
            Assert.AreEqual(expected, area2);
        }
        [Test]
        public void PoyntingSurfaceArea()
        {
            // Curved surface area of hemisphere, radius 3, above 2 X 2 square; area = 4.1610090517196.
            // This raises interesting point that what are these calculations confirmed against?
            //var result = LambdaNumerics.SurfaceArea2D((double x, double y) => Math.Sqrt(9 - x * x - y * y), (double x) => -1, (double x) => 1, -1, 1, 20, 20);
            //var nestedResult = LambdaNumerics.SurfaceArea2D((double x, double y) => LambdaNumerics.HemiSphere2D(x, y, 4.5), (double x) => -1, (double x) => 1, -1, 1, 20, 20);
            //var nestedResult3D = LambdaNumerics.SurfaceArea3D((double x, double y, double z) => LambdaNumerics.HemiSphere3D(x, y, z, 4.5), (double x) => -1, (double x) => 1, -1, 1, 20, 20);
            //const double expected = 4.1610090517196;
            //Assert.AreEqual(expected, result, Delta);
            //Assert.AreEqual(expected, nestedResult, Delta);
        }
    }
}
