namespace CartheurAnalytics
{
    /// <summary>
    /// A function of one variable.
    /// </summary>
    /// <param name="x">The x variable.</param>
    /// <returns></returns>
    public delegate double Function(double x);
    /// <summary>
    /// A function of two variables.
    /// </summary>
    /// <param name="x">The x variable.</param>
    /// <param name="y">The y variable</param>
    /// <returns></returns>
    public delegate double Functionxy(double x, double y);
    /// <summary>
    /// A function of three variables.
    /// </summary>
    /// <param name="x">The x variable.</param>
    /// <param name="y">The y variable.</param>
    /// <param name="z">The z variable.</param>
    public delegate double Functionxyz(double x, double y, double z);

}
