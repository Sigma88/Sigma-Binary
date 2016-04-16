using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;

using SigmaBinaryPlugin.Configuration;


namespace SigmaBinaryPlugin
{
    namespace Components
    {
        public class SigmaBinary : MonoBehaviour
        {
            // SigmaBinary
            
            public bool primaryLocked = false;
            public bool redrawOrbit = true;
            public string after;


            // Barycenter

            public string sbName;
            public string description;
            public bool selectable = true;


            // Bodies

            public CelestialBody sbPrimary;
            public CelestialBody sbBarycenter;
            public CelestialBody sbOrbit;


            void Start()
            {
                CelestialBody body = GetComponent<CelestialBody>();
                
                if (!string.IsNullOrEmpty(after))
                {
                    SigmaBinaryLoader.sigmabinaryLoadAfter.Add(after, body);
                    SigmaBinaryLoader.sigmabinaryName.Add(body.name, sbName);
                    SigmaBinaryLoader.sigmabinaryPrimaryLocked.Add(body.name, primaryLocked);
                    SigmaBinaryLoader.sigmabinaryRedrawOrbit.Add(body.name, redrawOrbit);
                    SigmaBinaryLoader.sigmabinaryDescription.Add(body.name, description);
                    SigmaBinaryLoader.sigmabinarySelectable.Add(body.name, selectable);
                }
                else
                {

                Start:

                    // Set sb CelestialBodies

                    sbPrimary = body.orbit.referenceBody;

                    foreach (CelestialBody sb in FlightGlobals.Bodies)
                    {
                        if (sb.bodyName == sbName)
                        {
                            sbBarycenter = sb;
                            SigmaBinaryLoader.ArchivesFixerList.Add(sb.name);
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
                            body.sphereOfInfluence = Math.Max(body.orbit.semiMajorAxis * Math.Pow(body.Mass / sbPrimary.Mass, 0.4), Math.Max(body.Radius * Kopernicus.Templates.SOIMinRadiusMult, body.Radius + Kopernicus.Templates.SOIMinAltitude));
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
                    sbBarycenter.GeeASL = (body.Mass + sbPrimary.Mass) * 6.674e-11d / Math.Pow(sbBarycenter.Radius, 2) / 9.81d;
                    sbBarycenter.rotationPeriod = body.orbit.period;
                    sbBarycenter.orbitDriver.orbitColor = sbPrimary.orbitDriver.orbitColor;

                    // Barycenter Properties
                    if (!selectable)
                        Kopernicus.Templates.notSelectable.Add(sbBarycenter.name);

                    if (string.IsNullOrEmpty(description))
                    {
                        sbBarycenter.bodyDescription = "This is the barycenter of the ";
                        if (sbPrimary.GetComponent<NameChanger>())
                            sbBarycenter.bodyDescription = sbBarycenter.bodyDescription + sbPrimary.GetComponent<NameChanger>().newName;
                        else
                            sbBarycenter.bodyDescription = sbBarycenter.bodyDescription + sbPrimary.name;
                        if (body.GetComponent<NameChanger>())
                            sbBarycenter.bodyDescription = sbBarycenter.bodyDescription + "-" + body.GetComponent<NameChanger>().newName + " system.";
                        else
                            sbBarycenter.bodyDescription = sbBarycenter.bodyDescription + "-" + body.name + " system.";
                    }
                    else
                    {
                        sbBarycenter.bodyDescription = description;
                    }

                    // Barycenter Orbit
                    if (SigmaBinaryLoader.sigmabinaryColor.ContainsKey(body.name))
                    {
                        sbBarycenter.orbitDriver.orbitColor = SigmaBinaryLoader.sigmabinaryColor[body.name];
                    }
                    else
                    {
                        sbBarycenter.orbitDriver.orbitColor = sbPrimary.orbitDriver.orbitColor;
                    }
                    if (Kopernicus.Templates.drawMode.ContainsKey(sbBarycenter.name))
                        Kopernicus.Templates.drawMode.Remove(sbBarycenter.name);
                    if (Kopernicus.Templates.drawIcons.ContainsKey(sbBarycenter.name))
                        Kopernicus.Templates.drawIcons.Remove(sbBarycenter.name);
                    if (SigmaBinaryLoader.sigmabinaryMode.ContainsKey(sbBarycenter.name))
                        Kopernicus.Templates.drawMode.Add(sbBarycenter.name, SigmaBinaryLoader.sigmabinaryMode[sbBarycenter.name]);
                    if (SigmaBinaryLoader.sigmabinaryIcon.ContainsKey(sbBarycenter.name))
                        Kopernicus.Templates.drawIcons.Add(sbBarycenter.name, SigmaBinaryLoader.sigmabinaryIcon[sbBarycenter.name]);

                    // Set Primary

                    if (sbPrimary.tidallyLocked)
                        sbPrimary.rotationPeriod = sbPrimary.orbit.period;
                    sbPrimary.tidallyLocked = false;
                    sbPrimary.orbitDriver.orbit = new Orbit(body.orbit.inclination, body.orbit.eccentricity, body.orbit.semiMajorAxis * body.Mass / (body.Mass + body.orbit.referenceBody.Mass), body.orbit.LAN, body.orbit.argumentOfPeriapsis + 180d, body.orbit.meanAnomalyAtEpoch, body.orbit.epoch, sbPrimary);
                    sbPrimary.orbit.referenceBody = sbBarycenter;
                    sbPrimary.orbit.period = body.orbit.period;
                    sbPrimary.orbit.ObTAtEpoch = body.orbit.ObTAtEpoch;
                    
                    if (Kopernicus.Templates.drawMode.ContainsKey(sbPrimary.name))
                        Kopernicus.Templates.drawMode.Remove(sbPrimary.name);
                    if (Kopernicus.Templates.drawIcons.ContainsKey(sbPrimary.name))
                        Kopernicus.Templates.drawIcons.Remove(sbPrimary.name);

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

                        if (body.GetComponent<NameChanger>())
                        {
                            if (Kopernicus.Templates.drawMode.ContainsKey(body.GetComponent<NameChanger>().oldName))
                                Kopernicus.Templates.drawMode.Remove(body.GetComponent<NameChanger>().oldName);
                            Kopernicus.Templates.drawMode.Add(body.GetComponent<NameChanger>().oldName, OrbitRenderer.DrawMode.REDRAW_AND_FOLLOW);
                        }
                        else
                        {
                            if (Kopernicus.Templates.drawMode.ContainsKey(body.name))
                                Kopernicus.Templates.drawMode.Remove(body.name);
                            Kopernicus.Templates.drawMode.Add(body.name, OrbitRenderer.DrawMode.REDRAW_AND_FOLLOW);
                        }
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
                        if (m.name != sbBarycenter.name && m.name != sbPrimary.name)
                        {
                            trackingstation.Add(m);
                        }
                        if (m.name == sbPrimary.name)
                        {
                            trackingstation.Add(PlanetariumCamera.fetch.targets.Find(d => d.name == sbBarycenter.name));
                            trackingstation.Add(PlanetariumCamera.fetch.targets.Find(d => d.name == sbPrimary.name));
                        }
                    }
                    PlanetariumCamera.fetch.targets.Clear();
                    PlanetariumCamera.fetch.targets.AddRange(trackingstation);

                    // Log
                    Debug.Log("--- BINARY SYSTEM LOADED ---\nReferenceBody: " + sbBarycenter.orbit.referenceBody.name + "\n   Barycenter: " + sbBarycenter.name + "\n      Primary: " + sbPrimary.name + "\n    Secondary: " + body.name);
                    
                    
                    if (SigmaBinaryLoader.sigmabinaryLoadAfter.ContainsKey(body.name))
                    {
                        body = SigmaBinaryLoader.sigmabinaryLoadAfter[body.name];
                        sbName = SigmaBinaryLoader.sigmabinaryName[body.name];
                        primaryLocked = SigmaBinaryLoader.sigmabinaryPrimaryLocked[body.name];
                        redrawOrbit = SigmaBinaryLoader.sigmabinaryRedrawOrbit[body.name];
                        description = SigmaBinaryLoader.sigmabinaryDescription[body.name];
                        selectable = SigmaBinaryLoader.sigmabinarySelectable[body.name];
                        goto Start;
                    }
                }
            }
        }
    }
}
