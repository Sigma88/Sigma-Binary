using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;



namespace SigmaBinaryPlugin
{
    public class KerbinFixer : MonoBehaviour
    {
        void Start()
        {
            if (SigmaBinary.kerbinFixer != null)
            {
                FlightGlobals.Bodies.Find(k => k.transform.name == "Kerbin").orbitDriver.orbit.referenceBody = FlightGlobals.Bodies.Find(rb => rb.transform.name == SigmaBinary.kerbinFixer);


                List<MapObject> trackingstation = new List<MapObject>();

                List<string> children = new List<string>();
                children.Add("Kerbin");

                for (int count = 1; count > 0;)
                {
                    foreach (CelestialBody b in FlightGlobals.Bodies)
                    {
                        count = 0;
                        if (children.Contains(b.referenceBody.name))
                        {
                            children.Add(b.name);
                            count++;
                        }
                    }
                }
                foreach (MapObject m in PlanetariumCamera.fetch.targets)
                {
                    if (!children.Contains(m.name) && m.name != SigmaBinary.kerbinFixer + "Orbit")
                    {
                        trackingstation.Add(m);
                    }

                    if (m.name == SigmaBinary.kerbinFixer)
                    {
                        foreach (string k in children)
                        {
                            trackingstation.Add(PlanetariumCamera.fetch.targets.Find(x => x.name == k));
                        }
                    }
                }
                PlanetariumCamera.fetch.targets.Clear();
                PlanetariumCamera.fetch.targets.AddRange(trackingstation);
                SigmaBinary.kerbinFixer = null;
            }
        }
    }
}