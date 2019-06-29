namespace SigmaBinaryPlugin
{
    internal static class Extensions
    {
        internal static double GetMass(this CelestialBody cb)
        {
            if (cb != null)
            {
                return cb.GeeASL * cb.Radius * cb.Radius / 6.67408E-11;
            }

            return 0;
        }
    }
}
