using System.Collections.Generic;
using UnityEngine;


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
                List<string> children = new List<string>();

                children.Add("Kerbin");

                for (int count = 1; count > 0;)
                {
                    foreach (CelestialBody b in FlightGlobals.Bodies)
                    {
                        count = 0;
                        if (children.Contains(b.referenceBody.transform.name))
                        {
                            children.Add(b.transform.name);
                            count++;
                        }
                    }
                }

                foreach (MapObject m in PlanetariumCamera.fetch.targets)
                {
                    if (m != null)
                    {
                        if (!children.Contains(m.celestialBody.transform.name))
                        {
                            trackingstation.Add(m);
                        }

                        if (m.celestialBody.transform.name == SigmaBinary.kerbinFixer)
                        {
                            foreach (string c in children)
                            {
                                trackingstation.Add(PlanetariumCamera.fetch.targets.Find(t => t.celestialBody.transform.name == c));
                            }
                        }
                    }
                }

                PlanetariumCamera.fetch.targets.Clear();
                PlanetariumCamera.fetch.targets.AddRange(trackingstation);
            }
        }
    }
}
