namespace SigmaBinaryPlugin
{
    internal static class Extensions
    {
        internal static double GetMass(this CelestialBody cb)
        {
            if (cb != null)
            {
                Debug.Log("GetMass", "CelestialBody = " + cb + ", gravParameter = " + cb.gravParameter + ", Mass = " + cb.Mass + ", GeeASL = " + cb.GeeASL + ", Radius = " + cb.Radius);
                return cb.GeeASL * 9.80665 * cb.Radius * cb.Radius / 6.67408E-11;
            }

            return 0;
        }
    }
}
