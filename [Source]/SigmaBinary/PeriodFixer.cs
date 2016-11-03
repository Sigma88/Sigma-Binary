using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;



namespace SigmaBinaryPlugin
{
    public class PeriodFixer : MonoBehaviour
    {
        void Start()
        {
            foreach (CelestialBody cb in FlightGlobals.Bodies)
            {
                if (SigmaBinary.periodFixerList.ContainsKey(cb.transform.name))
                {
                    cb.orbit.period = SigmaBinary.periodFixerList[cb.transform.name];
                    cb.orbit.meanMotion = 2 * Math.PI / cb.orbit.period;
                    cb.orbit.ObTAtEpoch = cb.orbit.meanAnomalyAtEpoch / 2 / Math.PI * cb.orbit.period;
                    if (cb.tidallyLocked)
                        cb.rotationPeriod = cb.orbit.period;


                    SigmaBinary.periodFixerList.Remove(cb.transform.name);
                }
            }
        }
    }
}
