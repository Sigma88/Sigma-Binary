using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class KerbinxFixer : MonoBehaviour
    {
        void Start()
        {
            Kopernicus.Events.OnPostFixing.Add(FixKerbins);
        }

        void FixKerbins()
        {
            Debug.Log("KerbinFixer", "'kerbinFixer' contains " + (SigmaBinary.kerbinFixer?.Count > 0 ? SigmaBinary.kerbinFixer.Count.ToString() : "no") + " bodies.");

            for (int i = 0; i < SigmaBinary.kerbinFixer?.Count; i++)
            {
                KeyValuePair<string, string> pair = SigmaBinary.kerbinFixer.ElementAt(i);
                CelestialBody body = FlightGlobals.Bodies.FirstOrDefault(cb => cb.transform.name == pair.Key);
                CelestialBody parent = FlightGlobals.Bodies.FirstOrDefault(cb => cb.transform.name == pair.Value);
                Debug.Log("KerbinFixer", "Body #" + (i + 1) + " = " + body + ", old parent = " + body.referenceBody + ", new parent = " + parent);

                body.orbit.referenceBody = parent;
                body.orbitDriver.orbit.referenceBody = parent;
                Debug.Log("KerbinFixer", "Changed 'referenceBody' of body " + body + " to " + body.referenceBody);
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class TrackingStationFixer : MonoBehaviour
    {
        void Start()
        {
            var targets = PlanetariumCamera.fetch.targets;
            Debug.Log("TrackingStationFixer", "targets.count before = " + targets.Count);
            List<MapObject> validTargets = new List<MapObject>();

            for (int i = 0; i < targets?.Count; i++)
            {
                Debug.Log("TrackingStationFixer", "targets[" + i + "] = " + targets[i] + ", celestialbody = " + targets[i]?.celestialBody + ", name = " + targets[i]?.celestialBody?.transform?.name);
                Debug.Log("TrackingStationFixer", "list contains name = " + SigmaBinary.trackingStationFixer.Contains(targets[i]?.celestialBody));
                if (SigmaBinary.trackingStationFixer.Contains(targets[i]?.celestialBody) != true)
                {
                    Debug.Log("TrackingStationFixer", "MapObject is valid.");
                    validTargets.Add(targets[i]);
                }
                else
                {
                    Debug.Log("TrackingStationFixer", "MapObject is NOT valid.");
                    DestroyImmediate(targets[i]);
                }
            }
            Debug.Log("TrackingStationFixer", "validTargets.count = " + validTargets.Count);

            PlanetariumCamera.fetch.targets.Clear();
            PlanetariumCamera.fetch.targets.AddRange(validTargets);
            Debug.Log("TrackingStationFixer", "targets.count after = " + targets.Count);

            for (int i = 0; i < SigmaBinary.kerbinFixer?.Count; i++)
            {
                KeyValuePair<string, string> pair = SigmaBinary.kerbinFixer.ElementAt(i);
                MapObject body = targets.FirstOrDefault(obj => obj.celestialBody.transform.name == pair.Key);
                MapObject parent = targets.FirstOrDefault(obj => obj.celestialBody.transform.name == pair.Value);

                List<MapObject> trackingstation = new List<MapObject>();
                List<MapObject> children = new List<MapObject>();
                children.Add(body);
                Debug.Log("TrackingStationFixer", "Body MapObject " + body + " added to 'children'.");

                for (int j = 0; j < targets?.Count; j++)
                {
                    MapObject target = targets[j];

                    // If the MapObject is not null
                    if (target != null)
                    {
                        // And the MapObject has not already been added to the list
                        if (!children.Any(obj => obj.celestialBody == target.celestialBody))
                        {
                            // And the MapObject is a child of any MapObject already in the list
                            if (children.Any(obj => obj.celestialBody == target.celestialBody.referenceBody))
                            {
                                children.Add(target);
                                Debug.Log("TrackingStationFixer", "Child MapObject " + target.celestialBody + " added to 'children'.");
                            }
                        }
                    }
                }

                // Reorder the tracking station targets
                Debug.Log("TrackingStationFixer", "Count of elements in 'trackingstation' = " + trackingstation.Count);
                for (int j = 0; j < targets.Count; j++)
                {
                    MapObject target = targets[j];

                    if (target != null)
                    {
                        if (!children.Contains(target))
                        {
                            trackingstation.Add(target);
                            Debug.Log("TrackingStationFixer", "Target MapObject " + target.celestialBody + " added to 'trackingstation'.");
                            Debug.Log("TrackingStationFixer", "New count of elements in 'trackingstation' = " + trackingstation.Count);
                        }
                        if (target == parent)
                        {
                            trackingstation.AddRange(children);
                            Debug.Log("TrackingStationFixer", children.Count + " child MapObjects added to 'trackingstation'.");
                            Debug.Log("TrackingStationFixer", "New count of elements in 'trackingstation' = " + trackingstation.Count);
                        }
                    }
                }

                PlanetariumCamera.fetch.targets.Clear();
                PlanetariumCamera.fetch.targets.AddRange(trackingstation);
            }
        }
    }
}
