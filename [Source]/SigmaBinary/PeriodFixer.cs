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
            CelestialBody body = GetComponent<CelestialBody>();
            Debug.Log("SigmaBinaryLog: Period Fixer");
            foreach (CelestialBody cb in FlightGlobals.Bodies)
            {
                cb.orbit.period = SigmaBinary.periodFixerList[body.transform.name];
                cb.orbit.meanMotion = 2 * Math.PI / body.orbit.period;
            }/*
            for (int i = 0; i < SigmaBinary.periodFixerList.Count(); i++)
            {
                Debug.Log("SigmaBinaryLog: i = " + i);
                Debug.Log("SigmaBinaryLog: OLD Period = " + SigmaBinary.periodFixerList.ElementAt(i).Key.orbit.period);
                SigmaBinary.periodFixerList.ElementAt(i).Key.
                SigmaBinary.periodFixerList.ElementAt(i).Key.orbit.meanMotion = 2 * Math.PI / SigmaBinary.periodFixerList.ElementAt(i).Key.orbit.period;
                SigmaBinary.periodFixerList.ElementAt(i).Key.orbit.ObTAtEpoch = SigmaBinary.periodFixerList.ElementAt(i).Key.orbit.ObTAtEpoch;
                Debug.Log("SigmaBinaryLog: Key = " + SigmaBinary.periodFixerList.ElementAt(i).Key.name);
                Debug.Log("SigmaBinaryLog: Value = " + SigmaBinary.periodFixerList.ElementAt(i).Value);
                Debug.Log("SigmaBinaryLog: NEW Period = " + SigmaBinary.periodFixerList.ElementAt(i).Key.orbit.period);
            }*/
            Debug.Log("SigmaBinaryLog: Period Fixer Ends Here");

        }
	}
}
