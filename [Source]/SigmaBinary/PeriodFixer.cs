using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;

using SigmaBinaryPlugin.Configuration;


namespace SigmaBinaryPlugin
{
	public class PeriodFixer : MonoBehaviour
	{
		void Start()
		{
            Debug.Log("SigmaBinaryLog: Period Fixer");
            CelestialBody body = GetComponent<CelestialBody>();
            Debug.Log("SigmaBinaryLog: 1");
            foreach (CelestialBody cb in FlightGlobals.Bodies)
            {
                Debug.Log("SigmaBinaryLog: 2");
                cb.orbit.period = SigmaBinary.periodFixerList[body.transform.name];
                Debug.Log("SigmaBinaryLog: 3");
                cb.orbit.meanMotion = 2 * Math.PI / body.orbit.period;
                Debug.Log("SigmaBinaryLog: 4");
            }
            Debug.Log("SigmaBinaryLog: Period Fixer Ends Here");
        }
	}
}
