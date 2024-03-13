using System;

namespace CartheurAnalytics
{
    /// <summary>
    /// For integration of function f over n subintervals from a to b.
    /// </summary>
    /// <param name="f">The f.</param>
    /// <param name="a">The beginning of the interval a.</param>
    /// <param name="b">The end of the interval b.</param>
    /// <param name="n">The number of subintervals.</param>
    public delegate double Integral(Function f, double a, double b, int n);

    public delegate double Integral3(Functionxyz f, double a, double b, double c, int n);

    /// <summary>
    /// Class containing integral and differential operators to perform numeric analysis (mapping and problem spaces) on electromagnetic phenomena.
    /// </summary>
    public partial class LambdaCalculus
    {
        //  Integration methods: Romberg and adaptive methods could be added.

        /// <summary>
        /// Simpson's rule integration of function f, over n (even) subintervals in the interval [a,b].
        /// </summary>
        public static Integral BasicSimpsonIntegration()
        {
            Integral integral = (f, a, b, n) =>
                {
                    // n, not even, throw exception code should go here
                    double sum = 0;
                    double h = (b - a) / n;
                    for (var i = 0; i < n; i = i + 2)
                        sum = sum + (f(a + i * h) + 4 * f(a + (i + 1) * h) + f(a + (i + 2) * h)) * h / 3;
                    return sum;
                };
            return integral;
        }
        /// <summary>
        /// 4-point Gaussian rule integration of the function f, over n subintervals in the interval (a,b).
        /// </summary>
        public static Integral GaussianRuleIntetration()
        {
            Integral integral = (f, a, b, n) =>
            {
                var weight = new double[4];
                weight[2] = 0.652145154862546;
                weight[1] = weight[2];
                weight[3] = 0.347854845137454;
                weight[0] = weight[3];

                var point = new double[4];
                point[3] = 0.861136311594053;
                point[0] = -point[3];
                point[2] = 0.339981043584856;
                point[1] = -point[2];

                double sum = 0;
                var h = (b - a) / n;

                for (var i = 0; i < n; i++)
                {
                    double s = 0;

                    var xl = a + i * h;
                    var xr = xl + h;

                    var m = (xr - xl) / 2;
                    var c = (xr + xl) / 2;

                    for (var j = 0; j < 4; j++)
                        s = s + weight[j] * f(m * point[j] + c) * m;

                    sum = sum + s;
                }
                return sum;
            };
            return integral;
        }
        /// <summary>
        /// 4-point Gaussian rule integration of the function f, over n subintervals in the interval (a,b).
        /// </summary>
        public static Integral3 GaussianRuleIntetration3()
        {
            Integral3 integral = (f, a, b, c, n) =>
            {
                var weight = new double[4];
                weight[2] = 0.652145154862546;
                weight[1] = weight[2];
                weight[3] = 0.347854845137454;
                weight[0] = weight[3];

                var point = new double[4];
                point[3] = 0.861136311594053;
                point[0] = -point[3];
                point[2] = 0.339981043584856;
                point[1] = -point[2];

                double sum = 0;
                var h = (b - a) / n;
                var h1 = (c - b) / n;
                var h2 = (c - a) / n;

                for (var i = 0; i < n; i++)
                {
                    double s = 0;

                    var xl = a + i * h;
                    var xr = xl + h;
                    var xm = (xr - xl) / 2;
                    var xd = (xr + xl) / 2;

                    var yl = b + i * h1;
                    var yr = yl + h1;
                    var ym = (yr - yl) / 2;
                    var yd = (yr + yl) / 2;

                    var zl = c + i * h2;
                    var zr = zl + h2;
                    var zm = (zr - zl) / 2;
                    var zd = (zr + zl) / 2;

                    for (var j = 0; j < 4; j++)
                        s = s + weight[j] * f(xm * point[j] + xd, ym * point[j] + yd, zm * point[j] + zd) * xm;

                    sum = sum + s;
                }
                return sum;
            };
            return integral;
        }

        // Double integration using nested lambda functions - method below could be extended to 3D or higher integration.

        /// <summary>
        /// 2D integration of a function of 2 variables. (( f(x,y)dxdy )).
        /// </summary>
        /// <param name="fxy">The fxy.</param>
        /// <param name="g">The g.</param>
        /// <param name="h">The h.</param>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="m">The m.</param>
        /// <param name="n">The n.</param>
        /// <param name="inInt">The in int.</param>
        /// <param name="outInt">The out int.</param>
        /// <remarks>Inner integral dx, from x=g(y) to x=h(y), over m subintervals, using algorithm inInt.
        /// Outer integral dy, from y=a to y=b, over n subintervals, using algorithm outInt.</remarks>
        public static double DoubleIntegration(Functionxy fxy, Function g, Function h, double a, double b, int m, int n, Integral inInt, Integral outInt)
        {
            return outInt((double y) => inInt((double x) => fxy(x, y), g(y), h(y), m), a, b, n);
        }
        /// <summary>
        /// 3D integration of a function of 3 variables. (( f(x,y,z) dxdydz )).
        /// </summary>
        /// <param name="fxyz">The fxyz.</param>
        /// <param name="g">The function g(x).</param>
        /// <param name="h">The function h(y).</param>
        /// <param name="i">The function i(z).</param>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <param name="m">The m.</param>
        /// <param name="n">The n.</param>
        /// <param name="inInt">The in int.</param>
        /// <param name="outInt">The out int.</param>
        /// <remarks>Inner integral dx, from x=g(y) to x=h(y), over m subintervals, using algorithm inInt.
        /// Outer integral dy, from y=a to y=b, over n subintervals, using algorithm outInt.
        /// Third integral dz, from z....</remarks>
        public static double TripleIntegration(Functionxyz fxyz, Function g, Function h, Function i, double a, double b, double c, int m, int n, Integral3 inInt, Integral3 outInt)// needs finishing
        {
            //return outInt((double u) => inInt((double x, double y, double z) => fxyz(x, y, z), g(u), h(u), i(u), m), a, b, c, n);
            return 0;
        }
        /// <summary>
        /// Calculates the surface area under z = fxy(x,y) from y=f(x) to y=g(x) and x=a to x=b.
        /// </summary>
        /// <param name="fxy">The function f(x,y).</param>
        /// <param name="f">The function f(x).</param>
        /// <param name="g">The function g(x).</param>
        /// <param name="a">The lower limit.</param>
        /// <param name="b">The upper limit.</param>
        /// <param name="m">The m.</param>
        /// <param name="n">The n.</param>
        public static double SurfaceArea2D(Functionxy fxy, Function f, Function g, double a, double b, int m, int n)
        {
            Integral intMethod = GaussianRuleIntetration();
            return DoubleIntegration((double x, double y) =>
                Math.Sqrt(1 + Math.Pow(PartialDifferentiationx(fxy, x, y), 2) + Math.Pow(PartialDifferentiationy(fxy, x, y), 2)), f, g, a, b, m, n, intMethod, intMethod);
        }
        public static double SurfaceArea3D(Functionxy fxyz, Function f, Function g, Function h, Function i, double a, double b, int m, int n)// needs finishing.
        {
            Integral3 intMethod = GaussianRuleIntetration3();
            //return TripleIntegration((double x, double y, double z) =>
                //Math.Sqrt(1 + Math.Pow(PartialDifferentiationx(fxyz, x, y), 2) + Math.Pow(PartialDifferentiationy(fxyz, x, y), 2) + Math.Pow(PartialDifferentiationz(fxyz, x, z), 2) + Math.Pow(PartialDifferentiationz(fxyz, y, z), 2)), f, g, i, a, b, m, n, intMethod, intMethod);
            return 0;
        }

    }
}
