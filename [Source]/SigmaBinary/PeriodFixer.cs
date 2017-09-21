using System;
using UnityEngine;


namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class PeriodFxixer : MonoBehaviour
    {
        void Start()
        {
            var bodies = FlightGlobals.Bodies;

            for (int i = 0; i < bodies?.Count; i++)
            {
                CelestialBody cb = bodies[i];

                if (SigmaBinary.periodFixerList.ContainsKey(cb.transform.name))
                {
                    Debug.Log("PeriodFixer", "Fixing orbital period of body " + cb + ", old orbital period = " + cb.orbit.period);
                    cb.orbit.period = SigmaBinary.periodFixerList[cb.transform.name];
                    cb.orbit.meanMotion = 2 * Math.PI / cb.orbit.period;
                    cb.orbit.ObTAtEpoch = cb.orbit.meanAnomalyAtEpoch / 2 / Math.PI * cb.orbit.period;
                    Debug.Log("PeriodFixer", "Fixed orbital period of body " + cb + ", new orbital period = " + cb.orbit.period);
                    if (cb.tidallyLocked)
                    {
                        cb.rotationPeriod = cb.orbit.period;
                        Debug.Log("PeriodFixer", "Fixed rotation period of tidallyLocked body " + cb + ", new rotation period = " + cb.rotationPeriod);
                    }
                }
            }
        }
    }
}
