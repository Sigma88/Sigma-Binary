using System;
using Kopernicus;
using Kopernicus.Configuration;


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

        internal static double GetOrbitalPeriod(this Body body, CelestialBody referenceBody)
        {
            if (body?.GeneratedBody?.orbitDriver?.orbit?.semiMajorAxis != null && referenceBody != null)
            {
                body.GeneratedBody.celestialBody.Set("customOrbitalPeriod", 2 * Math.PI * Math.Sqrt(Math.Pow(body.GeneratedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / referenceBody.GetMass()));
            }

            return body.GetOrbitalPeriod();
        }

        internal static double GetOrbitalPeriod(this Body body)
        {
            if (body?.GeneratedBody?.celestialBody?.Has("customOrbitalPeriod") == true)
            {
                return body.GeneratedBody.celestialBody.Get<double>("customOrbitalPeriod");
            }

            return 0;
        }
    }
}
