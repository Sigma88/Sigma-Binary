using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kopernicus;
using Kopernicus.Configuration;
using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using DrawMode = OrbitRendererBase.DrawMode;
using DrawIcons = OrbitRendererBase.DrawIcons;


namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    internal class SigmaBinary : MonoBehaviour
    {
        /// <summary> List of all objects of type 'Body'. </summary>
        internal static List<Body> ListOfBodies = new List<Body>();
        /// <summary> Dictionary of all secondary bodies. (Body.GeneratedBody.name, Body). </summary>
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
        internal static Dictionary<Body, EnumParser<DrawMode>> sigmabinaryMode = new Dictionary<Body, EnumParser<DrawMode>>();
        /// <summary> Dictionary holding the secondary Body and the orbit icon mode of the barycenter. </summary>
        internal static Dictionary<Body, EnumParser<DrawIcons>> sigmabinaryIcon = new Dictionary<Body, EnumParser<DrawIcons>>();

        void Start()
        {
            Events.OnBodyPostApply.Add(AddBodyToList);
            Events.OnLoaderLoadedAllBodies.Add(ProcessBinaries);
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
                Debug.Log("SigmaBinary.ProcessBinaries", "sbSecondary = " + sbSecondary?.Name);
                Body sbPrimary = OrbitPatcher(sbSecondary);
                Debug.Log("SigmaBinary.ProcessBinaries", "sbPrimary = " + sbPrimary?.Name);
                Body sbBarycenter = ListOfBodies.Find(b0 => b0.GeneratedBody.name == sigmabinarySBName[sbSecondary]);
                Debug.Log("SigmaBinary.ProcessBinaries", "sbBarycenter = " + sbBarycenter?.Name);
                Body sbReference = OrbitPatcher(sbPrimary);
                Debug.Log("SigmaBinary.ProcessBinaries", "sbReference = " + sbReference?.Name);
                Body sbOrbit = ListOfBodies.Find(ob => ob.GeneratedBody.name == sigmabinarySBName[sbSecondary] + "Orbit" && sigmabinaryRedrawOrbit.Contains(sbSecondary));
                Debug.Log("SigmaBinary.ProcessBinaries", "sbOrbit = " + sbOrbit?.Name);

                Debug.Log("SigmaBinary.ProcessBinaries", "Loaded Bodies\nReferenceBody: " + sbReference?.GeneratedBody?.name + "\n   Barycenter: " + sbBarycenter?.GeneratedBody?.name + "\n      Primary: " + sbPrimary?.GeneratedBody?.name + "\n    Secondary: " + sbSecondary?.GeneratedBody?.name + "\n        Orbit: " + sbOrbit?.GeneratedBody?.name);
                Debug.Log("SigmaBinary.ProcessBinaries", "Loaded PSystemBodies\nReferenceBody: " + sbReference?.GeneratedBody?.name + "\n   Barycenter: " + sbBarycenter?.GeneratedBody?.name + "\n      Primary: " + sbPrimary?.GeneratedBody?.name + "\n    Secondary: " + sbSecondary?.GeneratedBody?.name + "\n        Orbit: " + sbOrbit?.GeneratedBody?.name);
                // Check that the bodies exist

                if (sbPrimary == null || sbBarycenter == null || sbReference == null)
                    break;
                if (sbOrbit == null && sigmabinaryRedrawOrbit.Contains(sbSecondary))
                    break;


                // Load the CelestialBodies

                CelestialBody cbSecondary = sbSecondary?.GeneratedBody?.celestialBody;
                CelestialBody cbPrimary = sbPrimary?.GeneratedBody?.celestialBody;
                CelestialBody cbBarycenter = sbBarycenter?.GeneratedBody?.celestialBody;
                CelestialBody cbReference = sbReference?.GeneratedBody?.celestialBody;

                Debug.Log("SigmaBinary.ProcessBinaries", "Loaded CelestialBodies\nReferenceBody: " + cbReference + "\n   Barycenter: " + cbBarycenter + "\n      Primary: " + cbPrimary + "\n    Secondary: " + cbSecondary + "\n        Orbit: " + sbOrbit?.GeneratedBody?.celestialBody);



                /// Generating Binary System

                // Fix sphereOfInfluence where needed
                if (cbPrimary.Has("SBfixSOI"))
                {
                    cbPrimary.sphereOfInfluence = sbPrimary.GeneratedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbPrimary.GetMass() / cbReference.GetMass(), 0.4);
                    cbPrimary.Remove("SBfixSOI");
                    Debug.Log("SigmaBinary.ProcessBinaries", "Fixed 'sphereOfInfluence' of primary " + sbPrimary.GeneratedBody.name + ". sphereOfInfluence = " + cbPrimary.sphereOfInfluence);
                }
                if (cbSecondary.Has("SBfixSOI"))
                {
                    cbSecondary.sphereOfInfluence = sbSecondary.GeneratedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbSecondary.GetMass() / cbPrimary.GetMass(), 0.4);
                    cbSecondary.Remove("SBfixSOI");
                    Debug.Log("SigmaBinary.ProcessBinaries", "Fixed 'sphereOfInfluence' of secondary " + sbSecondary.GeneratedBody.name + ". sphereOfInfluence = " + cbSecondary.sphereOfInfluence);
                }


                // Remove Finalize Orbit

                if (cbSecondary.Has("finalizeBody") && cbSecondary.Get<bool>("finalizeBody"))
                {
                    cbSecondary.Set("finalizeBody", false);
                    Debug.Log("SigmaBinary.ProcessBinaries", "'finalizeBody' turned off for secondary body " + sbSecondary.GeneratedBody.name);
                    // Fix sphereOfInfluence
                    if (!cbSecondary.Has("sphereOfInfluence"))
                    {
                        cbSecondary.Set("sphereOfInfluence", Math.Max(sbSecondary.GeneratedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbSecondary.GetMass() / cbPrimary.GetMass(), 0.4), Math.Max(cbSecondary.Radius * Templates.SOI_MIN_RADIUS_MULTIPLIER, cbSecondary.Radius + Templates.SOI_MIN_ALTITUDE)));
                        Debug.Log("SigmaBinary.ProcessBinaries", "recalculated 'sphereOfInfluence' for secondary body " + sbSecondary.GeneratedBody.name);
                    }
                }
                if (cbPrimary.Has("finalizeBody") && cbPrimary.Get<bool>("finalizeBody"))
                {
                    cbPrimary.Set("finalizeBody", false);
                    Debug.Log("SigmaBinary.ProcessBinaries", "'finalizeBody' turned off for primary body " + sbPrimary.GeneratedBody.name);
                    // Fix sphereOfInfluence
                    if (!cbPrimary.Has("sphereOfInfluence"))
                    {
                        cbPrimary.Set("sphereOfInfluence", Math.Max(sbPrimary.GeneratedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbPrimary.GetMass() / cbReference.GetMass(), 0.4), Math.Max(cbPrimary.Radius * Templates.SOI_MIN_RADIUS_MULTIPLIER, cbPrimary.Radius + Templates.SOI_MIN_ALTITUDE)));
                        Debug.Log("SigmaBinary.ProcessBinaries", "recalculated 'sphereOfInfluence' for primary body " + sbPrimary.GeneratedBody.name);
                    }
                }



                /// Set Barycenter

                sbBarycenter.GeneratedBody.orbitDriver.orbit = new Orbit(sbPrimary.GeneratedBody.orbitDriver.orbit);
                sbBarycenter.Orbit.ReferenceBody = sbPrimary.Orbit.ReferenceBody;
                cbBarycenter.GeeASL = (cbPrimary.GetMass() + cbSecondary.GetMass()) / 1e5 * 6.674e-11d / Math.Pow(cbBarycenter.Radius, 2) / 9.80665d;
                cbBarycenter.rotationPeriod = 0;
                Debug.Log("SigmaBinary.SetBarycenter", "Printing orbital parameters of primary " + sbPrimary.GeneratedBody.name + " for reference.");
                Debug.Log("SigmaBinary.SetBarycenter", "referenceBody = " + sbPrimary.Orbit.ReferenceBody + ", semiMajorAxis = " + sbPrimary.GeneratedBody.orbitDriver.orbit.semiMajorAxis);
                Debug.Log("SigmaBinary.SetBarycenter", "Calculated new orbital parameters for barycenter " + sbBarycenter.GeneratedBody.name);
                Debug.Log("SigmaBinary.SetBarycenter", "referenceBody = " + sbBarycenter.Orbit.ReferenceBody + ", semiMajorAxis = " + sbBarycenter.GeneratedBody.orbitDriver.orbit.semiMajorAxis);

                if (periodFixerList.ContainsKey(sbPrimary.GeneratedBody.name))
                {
                    periodFixerList.Add(sbBarycenter.GeneratedBody.name, periodFixerList[sbPrimary.GeneratedBody.name]);
                    Debug.Log("SigmaBinary.SetBarycenter", "Added barycenter " + sbBarycenter.GeneratedBody.name + " to 'periodFixerList', used primary orbital period = " + periodFixerList[sbBarycenter.GeneratedBody.name]);
                }
                else
                {
                    periodFixerList.Add(sbBarycenter.GeneratedBody.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbPrimary.GeneratedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / cbReference.GetMass()));
                    Debug.Log("SigmaBinary.SetBarycenter", "Added barycenter " + sbBarycenter.GeneratedBody.name + " to 'periodFixerList', calculated orbital period = " + periodFixerList[sbBarycenter.GeneratedBody.name]);
                }

                // Orbit Color
                if (sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                {
                    sbBarycenter.GeneratedBody.orbitRenderer.SetColor(sigmabinaryOrbitColor[sbSecondary]);
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.GeneratedBody.name + " orbit line color set from list. color = " + sigmabinaryOrbitColor[sbSecondary]);
                }
                else
                {
                    sbBarycenter.GeneratedBody.orbitRenderer.orbitColor = sbPrimary.GeneratedBody.orbitRenderer.orbitColor;
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.GeneratedBody.name + " orbit line color copied from primary " + sbPrimary.GeneratedBody.name + ". color = " + sbPrimary.GeneratedBody.orbitRenderer.orbitColor);
                }

                // Icon Color
                if (sigmabinaryIconColor.ContainsKey(sbSecondary))
                {
                    sbBarycenter.GeneratedBody.orbitRenderer.nodeColor = sigmabinaryIconColor[sbSecondary];
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.GeneratedBody.name + " orbit icon color set from list. color = " + sigmabinaryIconColor[sbSecondary]);
                }
                else if (!sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                {
                    sbBarycenter.GeneratedBody.orbitRenderer.nodeColor = sbPrimary.GeneratedBody.orbitRenderer.nodeColor;
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.GeneratedBody.name + " orbit icon color copied from primary " + sbPrimary.GeneratedBody.name + ". color = " + sbPrimary.GeneratedBody.orbitRenderer.nodeColor);
                }

                // Description
                if (sigmabinaryDescription.ContainsKey(sbSecondary))
                {
                    cbBarycenter.bodyDescription = sigmabinaryDescription[sbSecondary].Replace("<name>", nameof(sbBarycenter)).Replace("<primary>", nameof(sbPrimary)).Replace("<secondary>", nameof(sbSecondary));
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.GeneratedBody.name + " description loaded.");
                }
                Debug.Log("SigmaBinary.SetBarycenter", "description = " + cbBarycenter.bodyDescription);

                // DrawMode and DrawIcons
                if (sigmabinaryMode.ContainsKey(sbSecondary))
                {
                    cbBarycenter.Set("drawMode", sigmabinaryMode[sbSecondary]);
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.GeneratedBody.name + " custom orbit 'drawMode' loaded. drawMode = " + sigmabinaryMode[sbSecondary].Value);
                }
                else if (cbPrimary.Has("drawMode"))
                {
                    cbBarycenter.Set("drawMode", cbPrimary.Get<DrawMode>("drawMode"));
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.GeneratedBody.name + " orbit 'drawMode' copied from primary " + sbPrimary.GeneratedBody.name + ". drawMode = " + cbPrimary.Get<DrawMode>("drawMode"));
                    cbPrimary.Set("drawMode", DrawMode.REDRAW_AND_RECALCULATE);
                    Debug.Log("SigmaBinary.SetBarycenter", "Primary " + sbPrimary.GeneratedBody.name + " orbit 'drawMode' automatically set. drawMode = " + DrawMode.REDRAW_AND_RECALCULATE);
                }
                if (sigmabinaryIcon.ContainsKey(sbSecondary))
                {
                    cbPrimary.Set("drawIcons", sigmabinaryIcon[sbSecondary]);
                    Debug.Log("SigmaBinary.SetBarycenter", "Primary " + sbPrimary.GeneratedBody.name + " custom orbit 'drawIcons' loaded. drawIcons = " + sigmabinaryIcon[sbSecondary].Value);
                }
                else if (cbPrimary.Has("drawIcons"))
                {
                    cbBarycenter.Set("drawIcons", cbPrimary.Get<DrawIcons>("drawIcons"));
                    Debug.Log("SigmaBinary.SetBarycenter", "Barycenter " + sbBarycenter.GeneratedBody.name + " orbit 'drawIcons' copied from primary " + sbPrimary.GeneratedBody.name + ". drawIcons = " + cbPrimary.Get<DrawIcons>("drawIcons"));
                    cbPrimary.Set("drawIcons", DrawIcons.ALL);
                    Debug.Log("SigmaBinary.SetBarycenter", "Primary " + sbPrimary.GeneratedBody.name + " orbit 'drawIcons' automatically set. drawIcons = " + DrawIcons.ALL);
                }



                /// Set Primary

                if (sbPrimary.Template.OriginalBody.celestialBody.name == "Kerbin")
                {
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.GeneratedBody.name + " uses Kerbin as Template.");
                    if (!kerbinFixer.ContainsKey(sbPrimary.GeneratedBody.name))
                    {
                        kerbinFixer.Add(sbPrimary.GeneratedBody.name, sbReference.GeneratedBody.name);
                        Debug.Log("SigmaBinary.SetPrimary", "Stored patched 'referenceBody' " + cbReference + " of Primary " + sbPrimary.GeneratedBody.name + " in 'kerbinFixer'.");
                    }
                    if (!archivesFixerList.ContainsKey(sbPrimary.GeneratedBody))
                    {
                        archivesFixerList.Add(sbPrimary.GeneratedBody, sbBarycenter.GeneratedBody);
                        Debug.Log("SigmaBinary.SetPrimary", "Stored primary " + sbPrimary.GeneratedBody.name + " and barycenter " + sbBarycenter.GeneratedBody.name + " in 'archivesFixerList'.");
                    }
                }
                sbPrimary.GeneratedBody.orbitDriver.orbit =
                    new Orbit
                    (
                        sbSecondary.GeneratedBody.orbitDriver.orbit.inclination,
                        sbSecondary.GeneratedBody.orbitDriver.orbit.eccentricity,
                        sbSecondary.GeneratedBody.orbitDriver.orbit.semiMajorAxis * cbSecondary.GetMass() / (cbSecondary.GetMass() + cbPrimary.GetMass()),
                        sbSecondary.GeneratedBody.orbitDriver.orbit.LAN,
                        sbSecondary.GeneratedBody.orbitDriver.orbit.argumentOfPeriapsis + 180d,
                        sbSecondary.GeneratedBody.orbitDriver.orbit.meanAnomalyAtEpoch,
                        sbSecondary.GeneratedBody.orbitDriver.orbit.epoch,
                        cbBarycenter
                    );
                sbPrimary.Orbit.ReferenceBody = sbBarycenter.GeneratedBody.name;
                Debug.Log("SigmaBinary.SetPrimary", "Printing masses of bodies for reference. primary = " + cbPrimary.GetMass() + ", secondary = " + cbSecondary.GetMass() + ", ratio = " + (cbPrimary.GetMass() / cbSecondary.GetMass()));
                Debug.Log("SigmaBinary.SetPrimary", "Printing orbital parameters of secondary " + sbSecondary.GeneratedBody.name + " for reference.");
                Debug.Log("SigmaBinary.SetPrimary", "referenceBody = " + sbSecondary.Orbit.ReferenceBody + ", semiMajorAxis = " + sbSecondary.GeneratedBody.orbitDriver.orbit.semiMajorAxis);
                Debug.Log("SigmaBinary.SetPrimary", "Calculated new orbital parameters for primary " + sbPrimary.GeneratedBody.name);
                Debug.Log("SigmaBinary.SetPrimary", "referenceBody = " + sbPrimary.Orbit.ReferenceBody + ", semiMajorAxis = " + sbPrimary.GeneratedBody.orbitDriver.orbit.semiMajorAxis + ", ratio = " + (sbSecondary.GeneratedBody.orbitDriver.orbit.semiMajorAxis / sbPrimary.GeneratedBody.orbitDriver.orbit.semiMajorAxis));

                if (periodFixerList.ContainsKey(sbPrimary.GeneratedBody.name))
                {
                    periodFixerList.Remove(sbPrimary.GeneratedBody.name);
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.GeneratedBody.name + " removed from 'periodFixerList'.");
                }
                periodFixerList.Add(sbPrimary.GeneratedBody.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.GeneratedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / cbPrimary.GetMass()));
                Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.GeneratedBody.name + " added to 'periodFixerList'. calculated orbital period = " + periodFixerList[sbPrimary.GeneratedBody.name]);

                // Primary Locked
                if (cbPrimary.solarRotationPeriod)
                {
                    cbPrimary.solarRotationPeriod = false;
                    cbPrimary.rotationPeriod = (periodFixerList[sbBarycenter.GeneratedBody.name] * cbPrimary.rotationPeriod) / (periodFixerList[sbBarycenter.GeneratedBody.name] + cbPrimary.rotationPeriod);
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.GeneratedBody.name + " 'solarRotationPeriod' set to 'false'. recalculated rotation period = " + cbPrimary.rotationPeriod);
                }
                if (sigmabinaryPrimaryLocked.Contains(sbSecondary))
                {
                    cbPrimary.solarRotationPeriod = false;
                    cbPrimary.rotationPeriod = periodFixerList[sbPrimary.GeneratedBody.name];
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.GeneratedBody.name + " is locked to reference " + sbReference.GeneratedBody.name + ".");
                    Debug.Log("SigmaBinary.SetPrimary", "Primary " + sbPrimary.GeneratedBody.name + " 'solarRotationPeriod' set to 'false'. recalculated rotation period = " + cbPrimary.rotationPeriod);
                }



                /// Set Secondary Orbit

                if (sigmabinaryRedrawOrbit.Contains(sbSecondary))
                {
                    mapViewFixerList.Add(sbOrbit.GeneratedBody.celestialBody, cbSecondary);

                    sbOrbit.GeneratedBody.orbitDriver.orbit =
                        new Orbit
                        (
                            sbSecondary.GeneratedBody.orbitDriver.orbit.inclination,
                            sbSecondary.GeneratedBody.orbitDriver.orbit.eccentricity,
                            sbSecondary.GeneratedBody.orbitDriver.orbit.semiMajorAxis - sbPrimary.GeneratedBody.orbitDriver.orbit.semiMajorAxis,
                            sbSecondary.GeneratedBody.orbitDriver.orbit.LAN,
                            sbSecondary.GeneratedBody.orbitDriver.orbit.argumentOfPeriapsis,
                            sbSecondary.GeneratedBody.orbitDriver.orbit.meanAnomalyAtEpoch,
                            sbSecondary.GeneratedBody.orbitDriver.orbit.epoch,
                            cbBarycenter
                        );
                    sbOrbit.Orbit.ReferenceBody = sbBarycenter.GeneratedBody.name;
                    Debug.Log("SigmaBinary.SetMarker", "Printing orbital parameters of primary " + sbPrimary.GeneratedBody.name + " for reference.");
                    Debug.Log("SigmaBinary.SetMarker", "referenceBody = " + sbPrimary.Orbit.ReferenceBody + ", semiMajorAxis = " + sbPrimary.GeneratedBody.orbitDriver.orbit.semiMajorAxis);
                    Debug.Log("SigmaBinary.SetMarker", "Printing orbital parameters of secondary " + sbSecondary.GeneratedBody.name + " for reference.");
                    Debug.Log("SigmaBinary.SetMarker", "referenceBody = " + sbSecondary.Orbit.ReferenceBody + ", semiMajorAxis = " + sbSecondary.GeneratedBody.orbitDriver.orbit.semiMajorAxis);
                    Debug.Log("SigmaBinary.SetMarker", "Calculated new orbital parameters for orbit marker " + sbOrbit.GeneratedBody.name);
                    Debug.Log("SigmaBinary.SetMarker", "referenceBody = " + sbOrbit.Orbit.ReferenceBody + ", semiMajorAxis = " + sbOrbit.GeneratedBody.orbitDriver.orbit.semiMajorAxis);

                    sbOrbit.GeneratedBody.orbitRenderer.orbitColor = sbSecondary.GeneratedBody.orbitRenderer.orbitColor;
                    Debug.Log("SigmaBinary.SetMarker", "Orbit marker " + sbOrbit.GeneratedBody.name + " orbit line color set from secondary " + sbSecondary.GeneratedBody.name + ". color = " + sbOrbit.GeneratedBody.orbitRenderer.orbitColor);

                    periodFixerList.Add(sbOrbit.GeneratedBody.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.GeneratedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / cbPrimary.GetMass()));
                    Debug.Log("SigmaBinary.SetMarker", "Orbit marker " + sbOrbit.GeneratedBody.name + " added to 'periodFixerList'. calculated orbital period = " + periodFixerList[sbOrbit.GeneratedBody.name]);

                    cbSecondary.Set("drawMode", DrawMode.OFF);
                    Debug.Log("SigmaBinary.SetMarker", "Secondary " + sbSecondary.GeneratedBody.name + " orbit 'drawMode' automatically set. drawMode = " + DrawMode.OFF);
                }



                /// Set SphereOfInfluence for Barycenter and Primary

                if (!cbPrimary.Has("sphereOfInfluence"))
                {
                    cbPrimary.Set("sphereOfInfluence", sbBarycenter.GeneratedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbPrimary.GetMass() / cbReference.GetMass(), 0.4));
                    Debug.Log("SigmaBinary.SetSoI", "Calculated 'sphereOfInfluence' for primary " + sbPrimary.GeneratedBody.name + ". sphereOfInfluence = " + cbPrimary.Get<double>("sphereOfInfluence"));
                }
                cbBarycenter.Set("sphereOfInfluence", cbPrimary.Get<double>("sphereOfInfluence"));
                Debug.Log("SigmaBinary.SetSoI", "Set barycenter " + sbBarycenter.GeneratedBody.name + " 'sphereOfInfluence' from primary " + sbPrimary.GeneratedBody.name + ". sphereOfInfluence = " + cbPrimary.Get<double>("sphereOfInfluence"));
                cbPrimary.Set("sphereOfInfluence", sbPrimary.GeneratedBody.orbitDriver.orbit.semiMajorAxis * (sbBarycenter.GeneratedBody.orbitDriver.orbit.eccentricity + 1) + cbBarycenter.Get<double>("sphereOfInfluence"));
                Debug.Log("SigmaBinary.SetSoI", "Recalculated 'sphereOfInfluence' for primary " + sbPrimary.GeneratedBody.name + ". sphereOfInfluence = " + cbPrimary.Get<double>("sphereOfInfluence"));



                /// Final Fixes for Bodies with a Kerbin Template

                // If the primary has a Kerbin Template, bypass PostSpawnOrbit
                if (kerbinFixer.ContainsKey(sbPrimary.GeneratedBody.name))
                {
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Primary " + sbPrimary.GeneratedBody.name + " uses Kerbin as Template.");

                    // Revert the referenceBody to the original one
                    sbPrimary.Orbit.ReferenceBody = kerbinFixer[sbPrimary.GeneratedBody.name];
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Primary " + sbPrimary.GeneratedBody.name + " 'referenceBody' reverted to the original. referenceBody = " + sbPrimary.Orbit.ReferenceBody);

                    // Save the PostSpawn referenceBody for later
                    kerbinFixer[sbPrimary.GeneratedBody.name] = sbBarycenter.GeneratedBody.name;
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "kerbinFixer[cbPrimary] set to barycenter " + sbBarycenter.GeneratedBody.name);

                    // Clear PostSpawnOrbit
                    if (sbPrimary.GeneratedBody.Has("orbitPatches"))
                    {
                        sbPrimary.GeneratedBody.Remove("orbitPatches");
                        Debug.Log("SigmaBinary.FixKerbinTemplate", "Primary " + sbPrimary.GeneratedBody.name + " 'PostSpawnOrbit' node removed, 'kerbinFixer' will handle this.");
                    }
                }

                // If the secondary has a Kerbin Template, restore PostSpawnOrbit referenceBody
                if (kerbinFixer.ContainsKey(sbSecondary.GeneratedBody.name))
                {
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Secondary " + sbSecondary.GeneratedBody.name + " uses Kerbin as Template.");
                    sbSecondary.Orbit.ReferenceBody = kerbinFixer[sbSecondary.GeneratedBody.name];
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Secondary " + sbSecondary.GeneratedBody.name + " 'referenceBody' reverted to the original. referenceBody = " + sbSecondary.Orbit.ReferenceBody);
                    kerbinFixer.Remove(sbSecondary.GeneratedBody.name);
                    Debug.Log("SigmaBinary.FixKerbinTemplate", "Secondary " + sbSecondary.GeneratedBody.name + " removed from 'kerbinFixer', 'PostSpawnOrbit' will handle this.");
                }



                /// Binary System Completed

                ListOfBinaries.Remove(ListOfBinaries.First().Key);
                Debug.Log("SigmaBinary.PostApply", "Binary system with secondary " + sbSecondary.GeneratedBody.name + " removed from 'ListOfBinaries'.");

                // Easter Eggs
                LateFixes.TextureFixer(sbPrimary, sbSecondary, ListOfBodies);

                // Log
                UnityEngine.Debug.Log("[SigmaLog]: Binary System Completed\nReferenceBody: " + sbReference.GeneratedBody.name + "\n   Barycenter: " + sbBarycenter.GeneratedBody.name + "\n      Primary: " + sbPrimary.GeneratedBody.name + "\n    Secondary: " + sbSecondary.GeneratedBody.name + (Debug.debug ? "\n        Orbit: " + sbOrbit?.GeneratedBody?.name : ""));
            }
            Debug.Log("SigmaBinary.ProcessBinaries", "Completed the set up of all binary systems.");
        }

        /// <summary>
        /// Gets the final name of a body
        /// </summary>
        string nameof(Body body)
        {
            return string.IsNullOrEmpty(body.CbNameLater) ? body.GeneratedBody.name : body.CbNameLater;
        }

        /// <summary>
        /// Applies the PostSpawnOrbit and then returns the referenceBody
        /// </summary>
        Body OrbitPatcher(Body body)
        {
            Debug.Log("SigmaBinary.OrbitPatcher", "");
            if (body?.GeneratedBody?.celestialBody?.Has("sbPatched") == false)
            {
                if (body.GeneratedBody?.orbitDriver?.orbit == null)
                {
                    Debug.Log("OrbitPatcher", "Body " + body.GeneratedBody.name + " does not have an orbit.");
                    return null;
                }

                // This 'if' is here to make sure stars don't give us trouble
                if (body.GeneratedBody?.orbitDriver?.orbit?.referenceBody == null)
                {
                    body.GeneratedBody.orbitDriver.orbit.referenceBody = ListOfBodies.Find(rb => rb.GeneratedBody.name == body.Orbit.ReferenceBody).GeneratedBody.celestialBody;
                    Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.GeneratedBody.name + " 'referenceBody' set to " + body.GeneratedBody.orbitDriver.orbit.referenceBody);
                }

                // If the body has a Kerbin Template, save the original referenceBody for later
                if (body.Template.OriginalBody.celestialBody.name == "Kerbin")
                {
                    Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.GeneratedBody.name + " uses Kerbin as Template.");

                    kerbinFixer.Add(body.GeneratedBody.name, body.Orbit.ReferenceBody);
                    Debug.Log("SigmaBinary.OrbitPatcher", "Store original 'referenceBody' " + body.Orbit.ReferenceBody + " of body " + body.GeneratedBody.name + " in 'kerbinFixer'.");
                }

                if (body.GeneratedBody.Has("orbitPatches"))
                {
                    ConfigNode patch = body.GeneratedBody.Get<ConfigNode>("orbitPatches");
                    Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.GeneratedBody.name + " has a 'PostSpawnOrbit' node.");

                    // Fix sphereOfInfluence
                    if (patch.HasValue("referenceBody") || patch.HasValue("semiMajorAxis"))
                    {
                        if (!body.GeneratedBody.celestialBody.Has("sphereOfInfluence"))
                        {
                            body.GeneratedBody.celestialBody.Set("SBfixSOI", true);
                            Debug.Log("SigmaBinary.OrbitPatcher", "'sphereOfInfluence' of body " + body.GeneratedBody.name + " needs to be recalculated.");
                        }
                    }

                    // Patch orbit using the original 'PostSpawnOrbit' node
                    if (patch?.values?.Count > 0)
                    {
                        // Create a new loader
                        OrbitLoader loader = new SigmaOrbitLoader(body.GeneratedBody);

                        // Apply the patch to the loader
                        Parser.LoadObjectFromConfigurationNode(loader, patch, "Kopernicus");
                        Debug.Log("SigmaBinary.OrbitPatcher", "Patched orbit of body " + body.GeneratedBody.name + " using 'PostSpawnOrbit' node");

                        // Remove 'PostSpawnOrbit' node
                        body.GeneratedBody.Remove("orbitPatches");
                        Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.GeneratedBody.name + " 'PostSpawnOrbit' node removed.");

                        // If the patch is used to reparent the body
                        if (patch.HasValue("referenceBody"))
                        {
                            body.Orbit.ReferenceBody = patch.GetValue("referenceBody");
                            body.GeneratedBody.orbitDriver.orbit.referenceBody = ListOfBodies.Find(rb => rb.GeneratedBody.name == body.Orbit.ReferenceBody).GeneratedBody.celestialBody;
                            Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.GeneratedBody.name + " 'referenceBody' set from 'PostSpawnOrbit'. referenceBody = " + body.Orbit.ReferenceBody + " (" + body.GeneratedBody.orbitDriver.orbit.referenceBody + ")");

                            // Keep the ConfigNode for all bodies with a Kerbin Template
                            if (body?.Template?.OriginalBody?.celestialBody?.name == "Kerbin")
                            {
                                Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.GeneratedBody.name + " uses Kerbin as Template.");

                                ConfigNode temp = new ConfigNode();
                                temp.AddValue("referenceBody", patch.GetValue("referenceBody"));
                                body.GeneratedBody.Set("orbitPatches", temp);
                                Debug.Log("SigmaBinary.OrbitPatcher", "Body " + body.GeneratedBody.name + " original 'PostSpawnOrbit' node restored.");
                            }
                        }
                    }
                }
                body.GeneratedBody.celestialBody.Set("sbPatched", true);
            }

            CelestialBody referenceBody = UBI.GetBody(body.Orbit.ReferenceBody, ListOfBodies.Select(b => b.CelestialBody).ToArray());
            return ListOfBodies?.FirstOrDefault(b => b?.CelestialBody == referenceBody);
        }
    }
}
