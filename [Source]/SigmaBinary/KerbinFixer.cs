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
                FlightGlobals.Bodies.Find(k => k.transform.name == "Kerbin").orbitDriver.orbit.referenceBody = FlightGlobals.Bodies.Find(rb => rb.transform.name == SigmaBinary.kerbinFixer);
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class TrackingStationFixer : MonoBehaviour
    {
        void Start()
        {
            if (SigmaBinary.kerbinFixer != null)
            {
                List<MapObject> trackingstation = new List<MapObject>();
                List<CelestialBody> children = new List<CelestialBody>();

                CelestialBody Kerbin = FlightGlobals.Bodies.Find(k => k.transform.name == "Kerbin");

                
                children.Add(Kerbin);


                for (int count = 1; count > 0;)
                {
                    foreach (CelestialBody b in FlightGlobals.Bodies)
                    {
                        count = 0;
                        if (children.Contains(b.referenceBody))
                        {
                            children.Add(b);
                            count++;
                        }
                    }
                }


                foreach (MapObject m in PlanetariumCamera.fetch.targets)
                {
                    if (m != null)
                    {
                        if (!children.Contains(m.celestialBody))
                        {
                            trackingstation.Add(m);
                        }

                        if (m.name == SigmaBinary.kerbinFixer)
                        {
                            foreach (CelestialBody c in children)
                            {
                                trackingstation.Add(PlanetariumCamera.fetch.targets.Find(x => x.celestialBody == c));
                            }
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