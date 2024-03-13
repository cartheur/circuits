using System;

namespace AnalogCircuitTests
{
    public class CalculusTests
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

        public void LambdaCalculusTest()
        {
            var integrate1 = LambdaCalculus.BasicSimpsonIntegration();
            var result = integrate1(Math.Sin, 0, Math.PI, 200);
            //  Console.WriteLine(integrate1(Sine, 0, Math.PI, 60));
            //Assert.AreEqual(2.00000008353986, result, 1E-2);

            var integrate2 = LambdaCalculus.GaussianRuleIntetration();
            var result2 = integrate2(Math.Sin, 0, Math.PI, 60);
            //  Console.WriteLine(integrate2(Sine, 0, Math.PI, 15));
            //Assert.AreEqual(2, result2, 1E-7);
            var result3 = integrate2(Sine, 0, Math.PI, 60);
            //  Console.WriteLine(integrate2(Math.Sin, 0, Math.PI, 15));
            //Assert.AreEqual(2, result3, 1E-7);
            //  Analytically, exactly 2 for the above integrals - note the superior convergence of Gauss 4-point!
            var result4 = LambdaCalculus.DoubleIntegration(P, F1, F2, 1, 2, 4, 4, integrate1, integrate2);
            //  Console.WriteLine(DoubleIntegration(P, F1, F2, 1, 2, 4, 4, integrate1, integrate2));
            //Assert.AreEqual(5.625, result4, 1E-7);
            //  Analytically 5 5/8.
            var result5 = LambdaCalculus.D5PointCenter()(Math.Sin, Math.PI);
            var result6 = LambdaCalculus.D5PointForward()(Math.Sin, Math.PI);
            var result7 = LambdaCalculus.DdPointCenter()(Math.Cos, Math.PI);
            //  Console.WriteLine(D5PointCenter()(Math.Sin, Math.PI));
            //  Console.WriteLine(D5PointForward()(Math.Sin, Math.PI));
            //  Console.WriteLine(DdPointCenter()(Math.Cos, Math.PI));
            var result8 = LambdaCalculus.CurveLength(F1, 0, Math.Sqrt(2), LambdaCalculus.GaussianRuleIntetration(), 2,
                                                     LambdaCalculus.D5PointCenter());
            //  Console.WriteLine(CurveLength(F1, 0, Math.Sqrt(2), GaussianRuleIntetration(), 2, D5PointCenter()));
            var result9 = 2 * LambdaCalculus.CurveLength(SemiCircle, -Math.Sqrt(2) / 2, Math.Sqrt(2) / 2,
                                                     LambdaCalculus.GaussianRuleIntetration(), 10,
                                                     LambdaCalculus.D5PointCenter());
            //  Console.WriteLine(2 * CurveLength(SemiCircle, -Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, GaussianRuleIntetration(), 10, D5PointCenter()));
            var result10 = LambdaCalculus.PartialDifferentiationx(Q, 5, 3);
            var result11 = LambdaCalculus.PartialDifferentiationy(Q, 5, 3);
            //  Console.WriteLine(PartialDifferentiationx(Q, 5, 3));
            //  Console.WriteLine(PartialDifferentiationy(Q, 5, 3));
            var result12 = LambdaCalculus.SurfaceArea2D(P, F1, F2, 1, 4, 4, 4);
            var result13 = LambdaCalculus.SurfaceArea2D(R, F1, F2, 1, 4, 4, 4);
            //Assert.AreEqual(7.5, result13, 1E-7);
            //  Console.WriteLine(SurfaceArea2D(P, F1, F2, 1, 4, 4, 4));
            //  Console.WriteLine(SurfaceArea2D(R, F1, F2, 1, 4, 4, 4));
            //  7.5 as expected
            var result14 = LambdaCalculus.SurfaceArea2D((x, y) => Math.Sqrt(9 - x * x - y * y), x => -1,
                                         x => 1, -1, 1, 20, 20);
            //  Console.WriteLine(SurfaceArea2D((double x,double y) => Math.Sqrt(9 - x * x - y * y), (double x) => -1, (double x) => 1, -1, 1, 20, 20));
            //Assert.AreEqual(4.1610090517196037, result14, 1E-7);
            //  A curved surface area of hemisphere, radius 3, above 2 X 2 square; area = 4.16...
            var arc1 = LambdaCalculus.Curvature(x => Math.Sqrt(4 - x * x), 1);
            var arc2 = LambdaCalculus.Curvature(x => (x + 2) * x * (x - 2), -1);
            var arc3 = LambdaCalculus.Curvature(x => (x + 2) * x * (x - 2), 1);
            var arc4 = LambdaCalculus.Curvature(x => (x + 2) * x * (x - 2), 0);
            //  Console.WriteLine(Curvature((double x) => Math.Sqrt(4 - x * x), 1));
            //  Console.WriteLine(Curvature((double x) => (x + 2) * x * (x - 2), -1));
            //  Console.WriteLine(Curvature((double x) => (x + 2) * x * (x - 2), 1));
            //  Console.WriteLine(Curvature((double x) => (x + 2) * x * (x - 2), 0));
            //Assert.AreEqual(2E-15, arc4, 1E-14);
            //  2E-15 (point of inflexion)
            var arc5 = LambdaCalculus.Curvature(x => x * Math.Sin(x), Math.PI * 3 / 2);
            //  Console.WriteLine(Curvature((double x) => x * Math.Sin(x), Math.PI * 3 / 2));
            var hold = "";
        }
    }
}
