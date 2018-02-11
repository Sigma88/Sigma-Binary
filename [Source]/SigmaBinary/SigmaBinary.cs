using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kopernicus;
using Kopernicus.Components;
using Kopernicus.Configuration;


namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    internal class SigmaBinary : MonoBehaviour
    {
        /// <summary> List of all objects of type 'Body'. </summary>
        internal static List<Body> ListOfBodies = new List<Body>();
        /// <summary> Dictionary of all secondary bodies. (Body.name, Body). </summary>
        internal static Dictionary<string, Body> ListOfBinaries = new Dictionary<string, Body>();

        /// <summary> Dictionary storing the names of the primary and reference bodies for postspawn fixes. <para>Only used for planets with a Kerbin Template.</para> </summary>
        internal static Dictionary<string, string> kerbinFixer = new Dictionary<string, string>();
        /// <summary> Dictionary holding the name of a body and the orbital period that needs to be assigned to that body. </summary>
        internal static Dictionary<string, double> periodFixerList = new Dictionary<string, double>();
        /// <summary> Dictionary holding primary and barycenter, needed for fixing the science archives post spawn. </summary>
        internal static Dictionary<PSystemBody, PSystemBody> archivesFixerList = new Dictionary<PSystemBody, PSystemBody>();
        /// <summary> Dictionary holding the CelestialBody of the orbital marker and the secondary body to fix the behaviour of the orbit lines. </summary>
        internal static Dictionary<CelestialBody, CelestialBody> mapViewFixerList = new Dictionary<CelestialBody, CelestialBody>();
        /// <summary> You should never want this to be true. </summary>
        internal static bool IamSad = (Environment.GetCommandLineArgs().Contains("-nyan-nyan") && Environment.GetCommandLineArgs().Contains("-NoFun"));

        /// <summary> Dictionary holding the names of two different secondary bodies. <para>The binary system of the first body needs to be processed before the one of the second body.</para> </summary>
        internal static Dictionary<string, Body> sigmabinaryLoadAfter = new Dictionary<string, Body>();
        /// <summary> Dictionary holding the secondary Body and the barycenter name. </summary>
        internal static Dictionary<Body, string> sigmabinarySBName = new Dictionary<Body, string>();
        /// <summary> List of secondary bodies in a binary system where the primary is tidally locked to the reference. </summary>
        internal static List<Body> sigmabinaryPrimaryLocked = new List<Body>();
        /// <summary> List of secondary bodies for which an orbit marker is required. </summary>
        internal static List<Body> sigmabinaryRedrawOrbit = new List<Body>();
        /// <summary> Dictionary holding the secondary Body and the description for the barycenter. </summary>
        internal static Dictionary<Body, string> sigmabinaryDescription = new Dictionary<Body, string>();
        /// <summary> Dictionary holding the secondary Body and the orbit line color of the barycenter. </summary>
        internal static Dictionary<Body, Color> sigmabinaryOrbitColor = new Dictionary<Body, Color>();
        /// <summary> Dictionary holding the secondary Body and the orbit icon color of the barycenter. </summary>
        internal static Dictionary<Body, Color> sigmabinaryIconColor = new Dictionary<Body, Color>();
        /// <summary> Dictionary holding the secondary Body and the orbit draw mode of the barycenter. </summary>
        internal static Dictionary<Body, EnumParser<OrbitRenderer.DrawMode>> sigmabinaryMode = new Dictionary<Body, EnumParser<OrbitRenderer.DrawMode>>();
        /// <summary> Dictionary holding the secondary Body and the orbit icon mode of the barycenter. </summary>
        internal static Dictionary<Body, EnumParser<OrbitRenderer.DrawIcons>> sigmabinaryIcon = new Dictionary<Body, EnumParser<OrbitRenderer.DrawIcons>>();

        void Start()
        {
            Kopernicus.Events.OnBodyPostApply.Add(AddBodyToList);
            Kopernicus.Events.OnLoaderLoadedAllBodies.Add(ProcessBinaries);
        }

        void AddBodyToList(Body body, ConfigNode node)
        {
            ListOfBodies.Add(body);
        }

        void ProcessBinaries(Loader ldr, ConfigNode cfgn)
        {
            if (ListOfBinaries?.Count > 0)
                Debug.Log("SigmaBinary.ProcessBinaries", "Starting the set up of " + ListOfBinaries?.Count + " binary systems");

            while (ListOfBinaries?.Count > 0)
            {
                /// Loading the Bodies

                Body sbSecondary = ListOfBinaries.First().Value;
                Body sbPrimary = OrbitPatcher(sbSecondary);
                Body sbBarycenter = ListOfBodies.Find(b0 => b0.name == sigmabinarySBName[sbSecondary]);
                Body sbReference = OrbitPatcher(sbPrimary);
                Body sbOrbit = ListOfBodies.Find(ob => ob.name == sigmabinarySBName[sbSecondary] + "Orbit" && sigmabinaryRedrawOrbit.Contains(sbSecondary));

                Debug.Log("SigmaBinary.ProcessBinaries", "Loaded Bodies\nReferenceBody: " + sbReference?.name + "\n   Barycenter: " + sbBarycenter?.name + "\n      Primary: " + sbPrimary?.name + "\n    Secondary: " + sbSecondary?.name + "\n        Orbit: " + sbOrbit?.name);
                Debug.Log("SigmaBinary.ProcessBinaries", "Loaded PSystemBodies\nReferenceBody: " + sbReference?.generatedBody + "\n   Barycenter: " + sbBarycenter?.generatedBody + "\n      Primary: " + sbPrimary?.generatedBody + "\n    Secondary: " + sbSecondary?.generatedBody + "\n        Orbit: " + sbOrbit?.generatedBody);
                // Check that the bodies exist

                if (sbPrimary == null || sbBarycenter == null || sbReference == null)
                    break;
                if (sbOrbit == null && sigmabinaryRedrawOrbit.Contains(sbSecondary))
                    break;


                // Load the CelestialBodies

                CelestialBody cbSecondary = sbSecondary?.generatedBody?.celestialBody;
                CelestialBody cbPrimary = sbPrimary?.generatedBody?.celestialBody;
                CelestialBody cbBarycenter = sbBarycenter?.generatedBody?.celestialBody;
                CelestialBody cbReference = sbReference?.generatedBody?.celestialBody;

                Debug.Log("SigmaBinary.ProcessBinaries", "Loaded CelestialBodies\nReferenceBody: " + cbReference + "\n   Barycenter: " + cbBarycenter + "\n      Primary: " + cbPrimary + "\n    Secondary: " + cbSecondary + "\n        Orbit: " + sbOrbit?.generatedBody?.celestialBody);



                /// Generating Binary System

                // Fix sphereOfInfluence where needed
                if (cbPrimary.Has("SBfixSOI"))
                {
                    cbPrimary.sphereOfInfluence = sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbPrimary.Mass / cbReference.Mass, 0.4);
                    cbPrimary.Remove("SBfixSOI");
                    Debug.Log("SigmaBinary.ProcessBinaries", "Fixed 'sphereOfInfluence' of primary " + sbPrimary.name + ". sphereOfInfluence = " + cbPrimary.sphereOfInfluence);
                }
                if (cbSecondary.Has("SBfixSOI"))
                {
                    cbSecondary.sphereOfInfluence = sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbSecondary.Mass / cbPrimary.Mass, 0.4);
                    cbSecondary.Remove("SBfixSOI");
                    Debug.Log("SigmaBinary.ProcessBinaries", "Fixed 'sphereOfInfluence' of secondary " + sbSecondary.name + ". sphereOfInfluence = " + cbSecondary.sphereOfInfluence);
                }


                // Remove Finalize Orbit

                if (cbSecondary.Has("finalizeBody") && cbSecondary.Get<bool>("finalizeBody"))
                {
                    cbSecondary.Set("finalizeBody", false);
                    Debug.Log("SigmaBinary.ProcessBinaries", "'finalizeBody' turned off for secondary body " + sbSecondary.name);
                    // Fix sphereOfInfluence
                    if (!cbSecondary.Has("sphereOfInfluence"))
                    {
                        cbSecondary.Set("sphereOfInfluence", Math.Max(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbSecondary.Mass / cbPrimary.Mass, 0.4), Math.Max(cbSecondary.Radius * Kopernicus.Templates.SOIMinRadiusMult, cbSecondary.Radius + Kopernicus.Templates.SOIMinAltitude)));
                        Debug.Log("SigmaBinary.ProcessBinaries", "recalculated 'sphereOfInfluence' for secondary body " + sbSecondary.name);
                    }
                }
                if (cbPrimary.Has("finalizeBody") && cbPrimary.Get<bool>("finalizeBody"))
                {
                    cbPrimary.Set("finalizeBody", false);
                    Debug.Log("SigmaBinary.ProcessBinaries", "'finalizeBody' turned off for primary body " + sbPrimary.name);
                    // Fix sphereOfInfluence
                    if (!cbPrimary.Has("sphereOfInfluence"))
                    {
                        cbPrimary.Set("sphereOfInfluence", Math.Max(sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbPrimary.Mass / cbReference.Mass, 0.4), Math.Max(cbPrimary.Radius * Kopernicus.Templates.SOIMinRadiusMult, cbPrimary.Radius + Kopernicus.Templates.SOIMinAltitude)));
                        Debug.Log("SigmaBinary.ProcessBinaries", "recalculated 'sphereOfInfluence' for primary body " + sbPrimary.name);
                    }
                }



                /// Set Barycenter

                sbBarycenter.generatedBody.orbitDriver.orbit = new Orbit(sbPrimary.generatedBody.orbitDriver.orbit);
                sbBarycenter.orbit.referenceBody = sbPrimary.orbit.referenceBody;
                cbBarycenter.GeeASL = (cbPrimary.Mass + cbSecondary.Mass) / 1e5 * 6.674e-11d / Math.Pow(cbBarycenter.Radius, 2) / 9.80665d;
                cbBarycenter.rotationPeriod = 0;
                Debug.Log("SigmaBinary.SetBarycenter", "Printing orbital parameters of primary " + sbPrimary.name + " for reference.");
                Debug.Log("SigmaBinary.SetBarycenter", "referenceBody = " + sbPrimary.orbit.referenceBody + ", semiMajorAxis = " + sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis);
                Debug.Log("SigmaBinary.SetBarycenter", "Calculated new orbital parameters for barycenter " + sbBarycenter.name);
                Debug.Log("SigmaBinary.SetBarycenter", "referenceBody = " + sbBarycenter.orbit.referenceBody + ", semiMajorAxis = " + sbBarycenter.generatedBody.orbitDriver.orbit.semiMajorAxis);

                if (periodFixerList.ContainsKey(sbPrimary.name))
                {
                    periodFixerList.Add(sbBarycenter.name, periodFixerList[sbPrimary.name]);
                    Debug.Log("SigmaBinary.SetBarycenter", "Added barycenter " + sbBarycenter.name + " to 'periodFixerList', used primary orbital period = " + periodFixerList[sbBarycenter.name]);
                }
                else
                {
                    periodFixerList.Add(sbBarycenter.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / cbReference.Mass));
                    Debug.Log("SigmaBinary.SetBarycenter", "Added barycenter " + sbBarycenter.name + " to 'periodFixerList', calculated orbital period = " + periodFixerList[sbBarycenter.name]);
                }

                // Orbit Color
                if (sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                {
                    sbBarycenter.generatedBody.orbitRenderer.SetColor(sigmabinaryOrbitColor[sbSecondary]);
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.name + " orbit line color set from list. color = " + sigmabinaryOrbitColor[sbSecondary]);
                }
                else
                {
                    sbBarycenter.generatedBody.orbitRenderer.orbitColor = sbPrimary.generatedBody.orbitRenderer.orbitColor;
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.name + " orbit line color copied from primary " + sbPrimary.name + ". color = " + sbPrimary.generatedBody.orbitRenderer.orbitColor);
                }

                // Icon Color
                if (sigmabinaryIconColor.ContainsKey(sbSecondary))
                {
                    sbBarycenter.generatedBody.orbitRenderer.nodeColor = sigmabinaryIconColor[sbSecondary];
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.name + " orbit icon color set from list. color = " + sigmabinaryIconColor[sbSecondary]);
                }
                else if (!sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                {
                    sbBarycenter.generatedBody.orbitRenderer.nodeColor = sbPrimary.generatedBody.orbitRenderer.nodeColor;
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.name + " orbit icon color copied from primary " + sbPrimary.name + ". color = " + sbPrimary.generatedBody.orbitRenderer.nodeColor);
                }

                // Description
                if (sigmabinaryDescription.ContainsKey(sbSecondary))
                {
                    cbBarycenter.bodyDescription = sigmabinaryDescription[sbSecondary].Replace("<name>", nameof(sbBarycenter)).Replace("<primary>", nameof(sbPrimary)).Replace("<secondary>", nameof(sbSecondary));
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.name + " description loaded.");
                }
                Debug.Log("SigmaBinary.SetBarycenter", "description = " + cbBarycenter.bodyDescription);

                // DrawMode and DrawIcons
                if (sigmabinaryMode.ContainsKey(sbSecondary))
                {
                    cbBarycenter.Set("drawMode", sigmabinaryMode[sbSecondary]);
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.name + " custom orbit 'drawMode' loaded. drawMode = " + sigmabinaryMode[sbSecondary].Value);
                }
                else if (cbPrimary.Has("drawMode"))
                {
                    cbBarycenter.Set("drawMode", cbPrimary.Get<OrbitRenderer.DrawMode>("drawMode"));
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.name + " orbit 'drawMode' copied from primary " + sbPrimary.name + ". drawMode = " + cbPrimary.Get<OrbitRenderer.DrawMode>("drawMode"));
                    cbPrimary.Set("drawMode", OrbitRenderer.DrawMode.REDRAW_AND_RECALCULATE);
                    Debug.Log("SigmaBinary.SetBarycenter", "Primary " + sbPrimary.name + " orbit 'drawMode' automatically set. drawMode = " + OrbitRenderer.DrawMode.REDRAW_AND_RECALCULATE);
                }
                if (sigmabinaryIcon.ContainsKey(sbSecondary))
                {
                    cbPrimary.Set("drawIcons", sigmabinaryIcon[sbSecondary]);
                    Debug.Log("SigmaBinary.SetBarycenter", "Primary " + sbPrimary.name + " custom orbit 'drawIcons' loaded. drawIcons = " + sigmabinaryIcon[sbSecondary].Value);
                }
                else if (cbPrimary.Has("drawIcons"))
                {
                    cbBarycenter.Set("drawIcons", cbPrimary.Get<OrbitRenderer.DrawIcons>("drawIcons"));
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.name + " orbit 'drawIcons' copied from primary " + sbPrimary.name + ". drawIcons = " + cbPrimary.Get<OrbitRenderer.DrawIcons>("drawIcons"));
                    cbPrimary.Set("drawIcons", OrbitRenderer.DrawIcons.ALL);
                    Debug.Log("SigmaBinary.SetBarycenter", "Primary " + sbPrimary.name + " orbit 'drawIcons' automatically set. drawIcons = " + OrbitRenderer.DrawIcons.ALL);
                }



                /// Set Primary

                if (sbPrimary.template.originalBody.celestialBody.name == "Kerbin")
                {
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.name + " uses Kerbin as Template.");
                    if (!kerbinFixer.ContainsKey(sbPrimary.name))
                    {
                        kerbinFixer.Add(sbPrimary.name, sbReference.name);
                        Debug.Log("SigmaBinary.SetPrimary", "Stored patched 'referenceBody' " + cbReference + " of Primary " + sbPrimary.name + " in 'kerbinFixer'.");
                    }
                    if (!archivesFixerList.ContainsKey(sbPrimary.generatedBody))
                    {
                        archivesFixerList.Add(sbPrimary.generatedBody, sbBarycenter.generatedBody);
                        Debug.Log("SigmaBinary.SetPrimary", "Stored primary " + sbPrimary.name + " and barycenter " + sbBarycenter.name + " in 'archivesFixerList'.");
                    }
                }
                sbPrimary.generatedBody.orbitDriver.orbit =
                    new Orbit
                    (
                        sbSecondary.generatedBody.orbitDriver.orbit.inclination,
                        sbSecondary.generatedBody.orbitDriver.orbit.eccentricity,
                        sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis * cbSecondary.Mass / (cbSecondary.Mass + cbPrimary.Mass),
                        sbSecondary.generatedBody.orbitDriver.orbit.LAN,
                        sbSecondary.generatedBody.orbitDriver.orbit.argumentOfPeriapsis + 180d,
                        sbSecondary.generatedBody.orbitDriver.orbit.meanAnomalyAtEpoch,
                        sbSecondary.generatedBody.orbitDriver.orbit.epoch,
                        cbBarycenter
                    );
                sbPrimary.orbit.referenceBody = sbBarycenter.name;
                Debug.Log("SigmaBinary.SetPrimary", "Printing masses of bodies for reference. primary = " + cbPrimary.Mass + ", secondary = " + cbSecondary.Mass + ", ratio = " + (cbPrimary.Mass / cbSecondary.Mass));
                Debug.Log("SigmaBinary.SetPrimary", "Printing orbital parameters of secondary " + sbSecondary.name + " for reference.");
                Debug.Log("SigmaBinary.SetPrimary", "referenceBody = " + sbSecondary.orbit.referenceBody + ", semiMajorAxis = " + sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis);
                Debug.Log("SigmaBinary.SetPrimary", "Calculated new orbital parameters for primary " + sbPrimary.name);
                Debug.Log("SigmaBinary.SetPrimary", "referenceBody = " + sbPrimary.orbit.referenceBody + ", semiMajorAxis = " + sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis + ", ratio = " + (sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis / sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis));

                if (periodFixerList.ContainsKey(sbPrimary.name))
                {
                    periodFixerList.Remove(sbPrimary.name);
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.name + " removed from 'periodFixerList'.");
                }
                periodFixerList.Add(sbPrimary.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / cbPrimary.Mass));
                Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.name + " added to 'periodFixerList'. calculated orbital period = " + periodFixerList[sbPrimary.name]);

                // Primary Locked
                if (cbPrimary.solarRotationPeriod)
                {
                    cbPrimary.solarRotationPeriod = false;
                    cbPrimary.rotationPeriod = (periodFixerList[sbBarycenter.name] * cbPrimary.rotationPeriod) / (periodFixerList[sbBarycenter.name] + cbPrimary.rotationPeriod);
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.name + " 'solarRotationPeriod' set to 'false'. recalculated rotation period = " + cbPrimary.rotationPeriod);
                }
                if (sigmabinaryPrimaryLocked.Contains(sbSecondary))
                {
                    cbPrimary.solarRotationPeriod = false;
                    cbPrimary.rotationPeriod = periodFixerList[sbPrimary.name];
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.name + " is locked to reference " + sbReference.name + ".");
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.name + " 'solarRotationPeriod' set to 'false'. recalculated rotation period = " + cbPrimary.rotationPeriod);
                }



                /// Set Secondary Orbit

                if (sigmabinaryRedrawOrbit.Contains(sbSecondary))
                {
                    mapViewFixerList.Add(sbOrbit.generatedBody.celestialBody, cbSecondary);

                    sbOrbit.generatedBody.orbitDriver.orbit =
                        new Orbit
                        (
                            sbSecondary.generatedBody.orbitDriver.orbit.inclination,
                            sbSecondary.generatedBody.orbitDriver.orbit.eccentricity,
                            sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis - sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis,
                            sbSecondary.generatedBody.orbitDriver.orbit.LAN,
                            sbSecondary.generatedBody.orbitDriver.orbit.argumentOfPeriapsis,
                            sbSecondary.generatedBody.orbitDriver.orbit.meanAnomalyAtEpoch,
                            sbSecondary.generatedBody.orbitDriver.orbit.epoch,
                            cbBarycenter
                        );
                    sbOrbit.orbit.referenceBody = sbBarycenter.name;
                    Debug.Log("SigmaBinary.SetMarker", "Printing orbital parameters of primary " + sbPrimary.name + " for reference.");
                    Debug.Log("SigmaBinary.SetMarker", "referenceBody = " + sbPrimary.orbit.referenceBody + ", semiMajorAxis = " + sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis);
                    Debug.Log("SigmaBinary.SetMarker", "Printing orbital parameters of secondary " + sbSecondary.name + " for reference.");
                    Debug.Log("SigmaBinary.SetMarker", "referenceBody = " + sbSecondary.orbit.referenceBody + ", semiMajorAxis = " + sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis);
                    Debug.Log("SigmaBinary.SetMarker", "Calculated new orbital parameters for orbit marker " + sbOrbit.name);
                    Debug.Log("SigmaBinary.SetMarker", "referenceBody = " + sbOrbit.orbit.referenceBody + ", semiMajorAxis = " + sbOrbit.generatedBody.orbitDriver.orbit.semiMajorAxis);

                    sbOrbit.generatedBody.orbitRenderer.orbitColor = sbSecondary.generatedBody.orbitRenderer.orbitColor;
                    Debug.Log("SigmaBinary.SetMarker", "Orbit marker " + sbOrbit.name + " orbit line color set from secondary " + sbSecondary.name + ". color = " + sbOrbit.generatedBody.orbitRenderer.orbitColor);

                    periodFixerList.Add(sbOrbit.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / cbPrimary.Mass));
                    Debug.Log("SigmaBinary.SetMarker", "Orbit marker " + sbOrbit.name + " added to 'periodFixerList'. calculated orbital period = " + periodFixerList[sbOrbit.name]);

                    cbSecondary.Set("drawMode", OrbitRenderer.DrawMode.OFF);
                    Debug.Log("SigmaBinary.SetMarker", "Secondary " + sbSecondary.name + " orbit 'drawMode' automatically set. drawMode = " + OrbitRenderer.DrawMode.OFF);
                }



                /// Set SphereOfInfluence for Barycenter and Primary

                if (!cbPrimary.Has("sphereOfInfluence"))
                {
                    cbPrimary.Set("sphereOfInfluence", sbBarycenter.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbPrimary.Mass / cbReference.Mass, 0.4));
                    Debug.Log("SigmaBinary.SetSoI", "Calculated 'sphereOfInfluence' for primary " + sbPrimary.name + ". sphereOfInfluence = " + cbPrimary.Get<double>("sphereOfInfluence"));
                }
                cbBarycenter.Set("sphereOfInfluence", cbPrimary.Get<double>("sphereOfInfluence"));
                Debug.Log("SigmaBinary.SetSoI", "Set barycenter " + sbBarycenter.name + " 'sphereOfInfluence' from primary " + sbPrimary.name + ". sphereOfInfluence = " + cbPrimary.Get<double>("sphereOfInfluence"));
                cbPrimary.Set("sphereOfInfluence", sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis * (sbBarycenter.generatedBody.orbitDriver.orbit.eccentricity + 1) + cbBarycenter.Get<double>("sphereOfInfluence"));
                Debug.Log("SigmaBinary.SetSoI", "Recalculated 'sphereOfInfluence' for primary " + sbPrimary.name + ". sphereOfInfluence = " + cbPrimary.Get<double>("sphereOfInfluence"));



                /// Final Fixes for Bodies with a Kerbin Template

                // If the primary has a Kerbin Template, bypass PostSpawnOrbit
                if (kerbinFixer.ContainsKey(sbPrimary.name))
                {
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Primary " + sbPrimary.name + " uses Kerbin as Template.");

                    // Revert the referenceBody to the original one
                    sbPrimary.orbit.referenceBody = kerbinFixer[sbPrimary.name];
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Primary " + sbPrimary.name + " 'referenceBody' reverted to the original. referenceBody = " + sbPrimary.orbit.referenceBody);

                    // Save the PostSpawn referenceBody for later
                    kerbinFixer[sbPrimary.name] = sbBarycenter.name;
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "kerbinFixer[cbPrimary] set to barycenter " + sbBarycenter.name);

                    // Clear PostSpawnOrbit
                    if (sbPrimary.generatedBody.Has("orbitPatches"))
                    {
                        sbPrimary.generatedBody.Remove("orbitPatches");
                        Debug.Log("SigmaBinary.FixKerbinTemplate", "Primary " + sbPrimary.name + " 'PostSpawnOrbit' node removed, 'kerbinFixer' will handle this.");
                    }
                }

                // If the secondary has a Kerbin Template, restore PostSpawnOrbit referenceBody
                if (kerbinFixer.ContainsKey(sbSecondary.name))
                {
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Secondary " + sbSecondary.name + " uses Kerbin as Template.");
                    sbSecondary.orbit.referenceBody = kerbinFixer[sbSecondary.name];
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Secondary " + sbSecondary.name + " 'referenceBody' reverted to the original. referenceBody = " + sbSecondary.orbit.referenceBody);
                    kerbinFixer.Remove(sbSecondary.name);
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Secondary " + sbSecondary.name + " removed from 'kerbinFixer', 'PostSpawnOrbit' will handle this.");
                }



                /// Binary System Completed

                ListOfBinaries.Remove(ListOfBinaries.First().Key);
                Debug.Log("SigmaBinary.PostApply", "Binary system with secondary " + sbSecondary.name + " removed from 'ListOfBinaries'.");

                // Easter Eggs
                LateFixes.TextureFixer(sbPrimary, sbSecondary, ListOfBodies);

                // Log
                UnityEngine.Debug.Log("[SigmaLog]: Binary System Completed\nReferenceBody: " + sbReference.name + "\n   Barycenter: " + sbBarycenter.name + "\n      Primary: " + sbPrimary.name + "\n    Secondary: " + sbSecondary.name + (Debug.debug ? "\n        Orbit: " + sbOrbit?.name : ""));
            }
            Debug.Log("SigmaBinary.ProcessBinaries", "Completed the set up of all binary systems.");
        }

        /// <summary>
        /// Gets the final name of a body
        /// </summary>
        string nameof(Body body)
        {
            return string.IsNullOrEmpty(body.cbNameLater) ? body.name : body.cbNameLater;
        }

        /// <summary>
        /// Applies the PostSpawnOrbit and then returns the referenceBody
        /// </summary>
        Body OrbitPatcher(Body body)
        {
            if (!body.generatedBody.celestialBody.Has("sbPatched"))
            {
                if (body.generatedBody?.orbitDriver?.orbit == null)
                {
                    Debug.Log("OrbitPatcher", "Body " + body.name + " does not have an orbit.");
                    return null;
                }

                // This 'if' is here to make sure stars don't give us trouble
                if (body.generatedBody?.orbitDriver?.orbit?.referenceBody == null)
                {
                    body.generatedBody.orbitDriver.orbit.referenceBody = ListOfBodies.Find(rb => rb.name == body.orbit.referenceBody).generatedBody.celestialBody;
                    Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.name + " 'referenceBody' set to " + body.generatedBody.orbitDriver.orbit.referenceBody);
                }

                // If the body has a Kerbin Template, save the original referenceBody for later
                if (body.template.originalBody.celestialBody.name == "Kerbin")
                {
                    Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.name + " uses Kerbin as Template.");

                    kerbinFixer.Add(body.name, body.orbit.referenceBody);
                    Debug.Log("SigmaBinary.OrbitPatcher", "Store original 'referenceBody' " + body.orbit.referenceBody + " of body " + body.name + " in 'kerbinFixer'.");
                }

                if (body.generatedBody.Has("orbitPatches"))
                {
                    ConfigNode patch = body.generatedBody.Get<ConfigNode>("orbitPatches");
                    Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.name + " has a 'PostSpawnOrbit' node.");

                    // Fix sphereOfInfluence
                    if (patch.HasValue("referenceBody") || patch.HasValue("semiMajorAxis"))
                    {
                        if (!body.generatedBody.celestialBody.Has("sphereOfInfluence"))
                        {
                            body.generatedBody.celestialBody.Set("SBfixSOI", true);
                            Debug.Log("SigmaBinary.OrbitPatcher", "'sphereOfInfluence' of body " + body.name + " needs to be recalculated.");
                        }
                    }

                    // Patch orbit using the original 'PostSpawnOrbit' node
                    if (patch?.values?.Count > 0)
                    {
                        // Create a new loader
                        OrbitLoader loader = new SigmaOrbitLoader(body.generatedBody);

                        // Apply the patch to the loader
                        Parser.LoadObjectFromConfigurationNode(loader, patch, "Kopernicus");
                        Debug.Log("SigmaBinary.OrbitPatcher", "Patched orbit of body " + body.name + " using 'PostSpawnOrbit' node");

                        // Remove 'PostSpawnOrbit' node
                        body.generatedBody.Remove("orbitPatches");
                        Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.name + " 'PostSpawnOrbit' node removed.");

                        // If the patch is used to reparent the body
                        if (patch.HasValue("referenceBody"))
                        {
                            body.orbit.referenceBody = patch.GetValue("referenceBody");
                            body.generatedBody.orbitDriver.orbit.referenceBody = ListOfBodies.Find(rb => rb.name == body.orbit.referenceBody).generatedBody.celestialBody;
                            Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.name + " 'referenceBody' set from 'PostSpawnOrbit'. referenceBody = " + body.orbit.referenceBody + " (" + body.generatedBody.orbitDriver.orbit.referenceBody + ")");

                            // Keep the ConfigNode for all bodies with a Kerbin Template
                            if (body?.template?.originalBody?.celestialBody?.name == "Kerbin")
                            {
                                Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.name + " uses Kerbin as Template.");

                                ConfigNode temp = new ConfigNode();
                                temp.AddValue("referenceBody", patch.GetValue("referenceBody"));
                                body.generatedBody.Set("orbitPatches", temp);
                                Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.name + " original 'PostSpawnOrbit' node restored.");
                            }
                        }
                    }
                }
                body.generatedBody.celestialBody.Set("sbPatched", true);
            }
            return ListOfBodies.FirstOrDefault(b => b.name == body.orbit.referenceBody);
        }
    }
}
