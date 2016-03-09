using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;


namespace SigmaBinaryPlugin
{
    namespace Components
    {
        public class SigmaBinary : MonoBehaviour
        {

            // SigmaBinary

            public bool sbEnabled = true;
            public bool primaryLocked = false;
            public bool redrawOrbit = true;


            // Barycenter

            public string sbName;
            public string description;


            // Bodies

            public CelestialBody sbPrimary;
            public CelestialBody sbBarycenter;
            public CelestialBody sbOrbit;


            void Start()
            {
                CelestialBody body = GetComponent<CelestialBody>();


                // Set sb CelestialBodies

                sbPrimary = body.orbit.referenceBody;

                foreach (CelestialBody sb in FlightGlobals.Bodies)
                {
                    if (sb.bodyName == sbName)
                    {
                        sbBarycenter = sb;
                    }
                }
                if (redrawOrbit)
                {
                    foreach (CelestialBody sb in FlightGlobals.Bodies)
                    {
                        if (sb.bodyName == sbBarycenter.bodyName + "Orbit")
                        {
                            sbOrbit = sb;
                        }
                    }
                }
                

                // Remove Finalize Orbit

                if (Kopernicus.Templates.finalizeBodies.Contains(body.name))
                {
                    Kopernicus.Templates.finalizeBodies.Remove(body.name);
                    // Fix sphereOfInfluence
                    if (!Kopernicus.Templates.sphereOfInfluence.ContainsKey(body.name))
                    {
                        body.sphereOfInfluence = Math.Max(body.orbit.semiMajorAxis * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4), Math.Max(body.Radius * Kopernicus.Templates.SOIMinRadiusMult, body.Radius + Kopernicus.Templates.SOIMinAltitude));
                    }
                }
                if (Kopernicus.Templates.finalizeBodies.Contains(sbPrimary.name))
                {
                    Kopernicus.Templates.finalizeBodies.Remove(sbPrimary.name);
                    // Fix sphereOfInfluence
                    if (!Kopernicus.Templates.sphereOfInfluence.ContainsKey(sbPrimary.name))
                    {
                        sbPrimary.sphereOfInfluence = Math.Max(sbPrimary.orbit.semiMajorAxis * Math.Pow(sbPrimary.Mass / sbPrimary.orbit.referenceBody.Mass, 0.4), Math.Max(sbPrimary.Radius * Kopernicus.Templates.SOIMinRadiusMult, sbPrimary.Radius + Kopernicus.Templates.SOIMinAltitude));
                    }
                }
                
                
                // Set Barycenter
                
                sbBarycenter.orbitDriver.orbit = new Orbit(sbPrimary.orbit.inclination, sbPrimary.orbit.eccentricity, sbPrimary.orbit.semiMajorAxis, sbPrimary.orbit.LAN, sbPrimary.orbit.argumentOfPeriapsis, sbPrimary.orbit.meanAnomalyAtEpoch, sbPrimary.orbit.epoch, body);
                sbBarycenter.orbit.referenceBody = sbPrimary.orbit.referenceBody;
                sbBarycenter.orbit.period = sbPrimary.orbit.period;
                sbBarycenter.orbit.ObTAtEpoch = sbPrimary.orbit.ObTAtEpoch;
                sbBarycenter.Mass = body.Mass + sbPrimary.Mass;
                sbBarycenter.rotationPeriod = body.orbit.period;
                sbBarycenter.orbitDriver.orbitColor = sbPrimary.orbitDriver.orbitColor;
                if (!Kopernicus.Templates.description.ContainsKey(sbBarycenter.name))
                {
                    sbBarycenter.description = "This is the barycenter of the ";
                    if (!Kopernicus.Templates.cbNameLater.ContainsKey(sbPrimary.name))
                    {
                        sbBarycenter.description = sbBarycenter.description + sbPrimary.cbNameLater;
                    }
                    else
                    {
                        sbBarycenter.description = sbBarycenter.description + sbPrimary.name;
                    }
                    if (!Kopernicus.Templates.cbNameLater.ContainsKey(body.name))
                    {
                        sbBarycenter.description = sbBarycenter.description + "-" + body.cbNameLater + " system.";
                    }
                    else
                    {
                        sbBarycenter.description = sbBarycenter.description + "-" + body.name + " system.";
                    }
                }
                
                
                // Set Primary
                
                sbPrimary.orbitDriver.orbit = new Orbit(body.orbit.inclination, body.orbit.eccentricity, body.orbit.semiMajorAxis * body.Mass / (body.Mass + body.orbit.referenceBody.Mass), body.orbit.LAN, body.orbit.argumentOfPeriapsis + 180d, body.orbit.meanAnomalyAtEpoch, body.orbit.epoch, sbPrimary);
                sbPrimary.orbit.referenceBody = sbBarycenter;
                sbPrimary.orbit.period = body.orbit.period;
                sbPrimary.orbit.ObTAtEpoch = body.orbit.ObTAtEpoch;

                if (primaryLocked)
                {
                    sbPrimary.rotationPeriod = body.orbit.period;
                }
                

                // Set Secondary Orbit
                if (redrawOrbit && sbOrbit)
                {
                    sbOrbit.orbitDriver.orbit = new Orbit(body.orbit.inclination, body.orbit.eccentricity, body.orbit.semiMajorAxis - sbPrimary.orbit.semiMajorAxis, body.orbit.LAN, body.orbit.argumentOfPeriapsis, body.orbit.meanAnomalyAtEpoch, body.orbit.epoch, sbOrbit);
                    sbOrbit.orbit.referenceBody = sbBarycenter;
                    sbOrbit.orbitDriver.orbitColor = body.orbitDriver.orbitColor;
                    if (Kopernicus.Templates.drawMode.ContainsKey(body.name))
                    {
                        Kopernicus.Templates.drawMode.Remove(body.name);
                    }
                    Kopernicus.Templates.drawMode.Add(body.name, OrbitRenderer.DrawMode.REDRAW_AND_FOLLOW);
                }
                

                // Set SphereOfInfluence for Barycenter and Primary

                if (Kopernicus.Templates.sphereOfInfluence.ContainsKey(sbPrimary.name))
                {
                    sbPrimary.sphereOfInfluence = Kopernicus.Templates.sphereOfInfluence[sbPrimary.name];
                    Kopernicus.Templates.sphereOfInfluence.Remove(sbPrimary.name);
                }
                sbBarycenter.sphereOfInfluence = sbPrimary.sphereOfInfluence;
                Kopernicus.Templates.sphereOfInfluence.Add(sbBarycenter.name, sbBarycenter.sphereOfInfluence);
                sbPrimary.sphereOfInfluence = sbPrimary.orbit.semiMajorAxis * (sbPrimary.orbit.eccentricity + 1) + sbBarycenter.sphereOfInfluence;

                
                // Reorder Trackingstation Bodies

                List<MapObject> trackingstation = new List<MapObject>();
                foreach (MapObject m in PlanetariumCamera.fetch.targets)
                {
                    if (m.name != sbBarycenter.name && m.name != sbPrimary.name && m.name != body.name)
                    {
                        trackingstation.Add(m);
                    }
                    if (m.name == sbPrimary.name)
                    {
                        trackingstation.Add(PlanetariumCamera.fetch.targets.Find(d => d.name == sbBarycenter.name));
                        trackingstation.Add(PlanetariumCamera.fetch.targets.Find(d => d.name == sbPrimary.name));
                        trackingstation.Add(PlanetariumCamera.fetch.targets.Find(d => d.name == body.name));
                    }
                }
                PlanetariumCamera.fetch.targets.Clear();
                PlanetariumCamera.fetch.targets.AddRange(trackingstation);
                

                // Log
                Debug.Log("--- BINARY SYSTEM LOADED ---\nReferenceBody: " + sbBarycenter.orbit.referenceBody.name + "\n   Barycenter: " + sbBarycenter.name + "\n      Primary: " + sbPrimary.name + "\n    Secondary: " + body.name);


            }
        }
    }
}

