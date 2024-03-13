using System;

namespace CartheurAnalytics
{
    /// <summary>
    /// Simpson's rule with the number of integration steps determined dynamically.
    /// Used for integrating a smooth function over a finite interval.
    /// The number of integration points is doubled at each step, while avoiding redundant function evaluations, until tolerance is met.
    /// </summary>
    public class SimpsonIntegrator
    {
        /// <summary>
        /// Calculates the value of a definite integral according to Simpson's Rule.
        /// </summary>
        /// <param name="f">integrand</param>
        /// <param name="a">left limit of integration</param>
        /// <param name="b">right limit of integration</param>
        /// <param name="desiredRelativeError">relative error tolerance</param>
        /// <returns>Integral value</returns>
        public static double Integrate(Func<double, double> f, double a, double b, double desiredRelativeError)
        {
            const int log2MaxFunctionEvals = 20;
            int functionEvalsUsed;
            double estimatedError;

            return Integrate(f, a, b, desiredRelativeError, log2MaxFunctionEvals, out functionEvalsUsed, out estimatedError);
        }
        /// <summary>
        /// Full interface. Gives a posteriori information, namely the mumber of function evaluations and a conservative estimate of the error.
        /// </summary>
        /// <param name="f">integrand</param>
        /// <param name="a">left limit of integration</param>
        /// <param name="b">right limit of integration</param>
        /// <param name="relativeErrorTolerance">relative error tolerance</param>
        /// <param name="log2MaxFunctionEvals">If this value is N, the number of function evaluations will be less than 2^n + 1</param>
        /// <param name="functionEvalsUsed">Actual number of function evaluations used</param>
        /// <param name="estimatedError">Conservative error estimate</param>
        /// <returns>Integral value</returns>
        public static double Integrate(Func<double, double> f, double a, double b, double relativeErrorTolerance, int log2MaxFunctionEvals, out int functionEvalsUsed, out double estimatedError)
        {
            var integral = 0.0;
            var previousContribution = 0.0;
            var previousIntegral = 0.0;
            var sum = 0.0;

            functionEvalsUsed = 0;
            estimatedError = double.MaxValue;

            for (var stage = 0; stage <= log2MaxFunctionEvals; stage++)
            {
                if (stage == 0)
                {
                    sum = f(a) + f(b);
                    functionEvalsUsed = 2;
                    integral = sum * 0.5 * (b - a);// <-- Settles to infinity here. How to quantify this?
                }
                else
                {
                    // Pattern of Simpson's rule coefficients:
                    //
                    // 1               1
                    // 1       4       1
                    // 1   4   2   4   1
                    // 1 4 2 4 2 4 2 4 1
                    // ...
                    //
                    // Each row multiplies new function evaluations by 4, and the evalutations from the previous step by 2.

                    var numNewPts = 1 << (stage - 1);
                    var mostRecentContribution = 0.0;
                    var h = (b - a) / numNewPts;
                    var x = a + 0.5 * h;
                    for (var i = 0; i < numNewPts; i++)
                        mostRecentContribution += f(x + i * h);
                    functionEvalsUsed += numNewPts;
                    mostRecentContribution *= 4.0;
                    sum += mostRecentContribution - 0.5 * previousContribution;

                    integral = sum * (b - a) / ((1 << stage) * 3.0);

                    // Require at least five stages to reduce the risk of incorrectly declaring convergence too soon.
                    // Note that you can specify fewer stages, but the early termination rule below will not be used.
                    if (stage >= 5)
                    {
                        estimatedError = Math.Abs(integral - previousIntegral); // conservative
                        if (estimatedError <= relativeErrorTolerance * Math.Abs(previousIntegral))
                            return integral;
                    }
                    previousContribution = mostRecentContribution;
                    previousIntegral = integral;
                }
            }
            return integral;
        }
    }
}
