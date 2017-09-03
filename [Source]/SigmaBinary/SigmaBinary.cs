using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Kopernicus.Components;
using Kopernicus.Configuration;
using Kopernicus;
using Contracts;


namespace SigmaBinaryPlugin
{
    [ParserTargetExternal("Body", "Properties", "Kopernicus")]
    public class SigmaBinary : BaseLoader, IParserEventSubscriber
    {
        public static List<Body> ListOfBodies = new List<Body>();
        public static Dictionary<string, Body> ListOfBinaries = new Dictionary<string, Body>();

        public static Dictionary<string, string[]> archivesFixerList = new Dictionary<string, string[]>();
        public static Dictionary<string, double> periodFixerList = new Dictionary<string, double>();
        public static Dictionary<string, string> mapViewFixerList = new Dictionary<string, string>();
        public static string kerbinFixer;
        public static bool IamSad = (Environment.GetCommandLineArgs().Contains("-nyan-nyan") && Environment.GetCommandLineArgs().Contains("-NoFun"));

        public static Dictionary<string, Body> sigmabinaryLoadAfter = new Dictionary<string, Body>();
        public static Dictionary<Body, string> sigmabinarySBName = new Dictionary<Body, string>();
        public static List<Body> sigmabinaryPrimaryLocked = new List<Body>();
        public static List<Body> sigmabinaryRedrawOrbit = new List<Body>();
        public static Dictionary<Body, string> sigmabinaryDescription = new Dictionary<Body, string>();
        public static Dictionary<Body, Color> sigmabinaryOrbitColor = new Dictionary<Body, Color>();
        public static Dictionary<Body, Color> sigmabinaryIconColor = new Dictionary<Body, Color>();
        public static Dictionary<Body, EnumParser<OrbitRenderer.DrawMode>> sigmabinaryMode = new Dictionary<Body, EnumParser<OrbitRenderer.DrawMode>>();
        public static Dictionary<Body, EnumParser<OrbitRenderer.DrawIcons>> sigmabinaryIcon = new Dictionary<Body, EnumParser<OrbitRenderer.DrawIcons>>();


        void IParserEventSubscriber.Apply(ConfigNode node)
        {
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {

            ListOfBodies.Add(Loader.currentBody);

            for (int i = 0; i < ListOfBinaries.Count;)
            {
                /// Loading the Bodies

                Body sbSecondary = ListOfBinaries.First().Value;
                Body sbPrimary = ListOfBodies.Find(b1 => b1.name == OrbitPatcher(sbSecondary));
                Body sbBarycenter = ListOfBodies.Find(b0 => b0.name == sigmabinarySBName[sbSecondary]);
                Body sbReference = ListOfBodies.Find(rb => rb.name == OrbitPatcher(sbPrimary));
                Body sbOrbit = ListOfBodies.Find(ob => ob.name == sigmabinarySBName[sbSecondary] + "Orbit" && sigmabinaryRedrawOrbit.Contains(sbSecondary));

                if (archivesFixerList.ContainsKey(sbPrimary.name))
                    sbReference = ListOfBodies.Find(rb => rb.name == archivesFixerList[sbPrimary.name][1]);

                // Check that the bodies exist

                if (sbPrimary == null || sbBarycenter == null || sbReference == null)
                    break;
                if (sbOrbit == null && sigmabinaryRedrawOrbit.Contains(sbSecondary))
                    break;

                // Load the CelestialBodies

                CelestialBody cbSecondary = sbSecondary.generatedBody.celestialBody;
                CelestialBody cbPrimary = sbPrimary.generatedBody.celestialBody;
                CelestialBody cbBarycenter = sbBarycenter.generatedBody.celestialBody;
                CelestialBody cbReference = sbReference.generatedBody.celestialBody;



                /// Generating Binary System


                // Remove Finalize Orbit

                if (cbSecondary.Has("finalizeBody") && cbSecondary.Get<bool>("finalizeBody"))
                {
                    cbSecondary.Set("finalizeBody", false);
                    // Fix sphereOfInfluence
                    if (!cbSecondary.Has("sphereOfInfluence"))
                    {
                        cbSecondary.Set("sphereOfInfluence", Math.Max(sbSecondary.orbit.semiMajorAxis * Math.Pow(cbSecondary.Mass / cbPrimary.Mass, 0.4), Math.Max(cbSecondary.Radius * Kopernicus.Templates.SOIMinRadiusMult, cbSecondary.Radius + Kopernicus.Templates.SOIMinAltitude)));
                    }
                }
                if (cbPrimary.Has("finalizeBody") && cbPrimary.Get<bool>("finalizeBody"))
                {
                    cbPrimary.Set("finalizeBody", false);
                    // Fix sphereOfInfluence
                    if (!cbPrimary.Has("sphereOfInfluence"))
                    {
                        cbPrimary.Set("sphereOfInfluence", Math.Max(sbPrimary.orbit.semiMajorAxis * Math.Pow(cbPrimary.Mass / cbReference.Mass, 0.4), Math.Max(cbPrimary.Radius * Kopernicus.Templates.SOIMinRadiusMult, cbPrimary.Radius + Kopernicus.Templates.SOIMinAltitude)));
                    }
                }



                /// Set Barycenter

                sbBarycenter.generatedBody.orbitDriver.orbit = new Orbit(sbPrimary.generatedBody.orbitDriver.orbit);
                sbBarycenter.orbit.referenceBody = sbPrimary.orbit.referenceBody;
                cbBarycenter.GeeASL = (cbPrimary.Mass + cbSecondary.Mass) / 1e5 * 6.674e-11d / Math.Pow(cbBarycenter.Radius, 2) / 9.80665d;
                cbBarycenter.rotationPeriod = 0;

                if (periodFixerList.ContainsKey(sbPrimary.name))
                    periodFixerList.Add(sbBarycenter.name, periodFixerList[sbPrimary.name]);
                else
                    periodFixerList.Add(sbBarycenter.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / cbReference.Mass));


                // Orbit Color

                if (sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                    sbBarycenter.generatedBody.orbitRenderer.SetColor(sigmabinaryOrbitColor[sbSecondary]);
                else
                    sbBarycenter.generatedBody.orbitRenderer.orbitColor = sbPrimary.generatedBody.orbitRenderer.orbitColor;


                // Icon Color

                if (sigmabinaryIconColor.ContainsKey(sbSecondary))
                    sbBarycenter.generatedBody.orbitRenderer.nodeColor = sigmabinaryIconColor[sbSecondary];
                else if (!sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                    sbBarycenter.generatedBody.orbitRenderer.nodeColor = sbPrimary.generatedBody.orbitRenderer.nodeColor;


                // Description

                if (!sigmabinaryDescription.ContainsKey(sbSecondary))
                {
                    cbBarycenter.bodyDescription = "This is the barycenter of the ";
                    if (cbPrimary.GetComponent<NameChanger>() != null)
                        cbBarycenter.bodyDescription = cbBarycenter.bodyDescription + cbPrimary.GetComponent<NameChanger>().newName;
                    else
                        cbBarycenter.bodyDescription = cbBarycenter.bodyDescription + sbPrimary.name;
                    if (cbSecondary.GetComponent<NameChanger>() != null)
                        cbBarycenter.bodyDescription = cbBarycenter.bodyDescription + "-" + cbSecondary.GetComponent<NameChanger>().newName + " system.";
                    else
                        cbBarycenter.bodyDescription = cbBarycenter.bodyDescription + "-" + cbSecondary.name + " system.";
                }
                else
                    cbBarycenter.bodyDescription = sigmabinaryDescription[sbSecondary];


                // DrawMode and DrawIcons

                if (sigmabinaryMode.ContainsKey(sbSecondary))
                {
                    cbBarycenter.Set("drawMode", sigmabinaryMode[sbSecondary]);
                }
                else if (cbPrimary.Has("drawMode"))
                {
                    cbBarycenter.Set("drawMode", cbPrimary.Get<OrbitRenderer.DrawMode>("drawMode"));
                    cbPrimary.Set("drawMode", OrbitRenderer.DrawMode.REDRAW_AND_RECALCULATE);
                }
                if (sigmabinaryIcon.ContainsKey(sbSecondary))
                {
                    cbPrimary.Set("drawIcons", sigmabinaryIcon[sbSecondary]);
                }
                else if (cbPrimary.Has("drawIcons"))
                {
                    cbBarycenter.Set("drawIcons", cbPrimary.Get<OrbitRenderer.DrawIcons>("drawIcons"));
                    cbPrimary.Set("drawIcons", OrbitRenderer.DrawIcons.ALL);
                }



                /// Set Primary

                if (!archivesFixerList.ContainsKey(sbPrimary.name))
                    archivesFixerList.Add(sbPrimary.name, new string[] { sbBarycenter.name, sbReference.name });
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

                if (periodFixerList.ContainsKey(sbPrimary.name))
                    periodFixerList.Remove(sbPrimary.name);
                periodFixerList.Add(sbPrimary.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / cbPrimary.Mass));


                // Primary Locked

                if (cbPrimary.solarRotationPeriod)
                {
                    cbPrimary.solarRotationPeriod = false;
                    cbPrimary.rotationPeriod = (periodFixerList[sbBarycenter.name] * cbPrimary.rotationPeriod) / (periodFixerList[sbBarycenter.name] + cbPrimary.rotationPeriod);
                }
                if (sigmabinaryPrimaryLocked.Contains(sbSecondary))
                {
                    cbPrimary.solarRotationPeriod = false;
                    cbPrimary.rotationPeriod = periodFixerList[sbPrimary.name];
                }



                /// Set Secondary Orbit

                if (sigmabinaryRedrawOrbit.Contains(sbSecondary))
                {

                    mapViewFixerList.Add(sbOrbit.name, sbSecondary.name);

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
                    sbOrbit.generatedBody.orbitRenderer.orbitColor = sbSecondary.generatedBody.orbitRenderer.orbitColor;


                    periodFixerList.Add(sbOrbit.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / cbPrimary.Mass));


                    cbSecondary.Set("drawMode", OrbitRenderer.DrawMode.OFF);

                }



                /// Set SphereOfInfluence for Barycenter and Primary

                if (!cbPrimary.Has("sphereOfInfluence"))
                {
                    cbPrimary.Set("sphereOfInfluence", sbBarycenter.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(cbPrimary.Mass / cbReference.Mass, 0.4));
                }
                cbBarycenter.Set("sphereOfInfluence", cbPrimary.Get<double>("sphereOfInfluence"));
                cbPrimary.Set("sphereOfInfluence", sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis * (sbBarycenter.generatedBody.orbitDriver.orbit.eccentricity + 1) + cbBarycenter.Get<double>("sphereOfInfluence"));


                if (sbPrimary.name == "Kerbin")
                {
                    // Bypass PostSpawnOrbit
                    cbPrimary.Set("orbitPatches", new ConfigNode());
                    kerbinFixer = sbPrimary.orbit.referenceBody;
                    sbPrimary.orbit.referenceBody = "Sun";
                }
                if (sbSecondary.name == "Kerbin")
                {
                    // Let Kopernicus handle this with PostSpawnOrbit
                    sbPrimary.orbit.referenceBody = "Sun";
                }



                /// Binary System Completed

                ListOfBinaries.Remove(ListOfBinaries.First(x => x.Value == sbSecondary).Key);
                LateFixes.TextureFixer(sbPrimary, sbSecondary, ListOfBodies);

                // Log
                Debug.Log("\nSigmaBinaryLog:\n\n--- BINARY SYSTEM LOADED ---\nReferenceBody: " + sbReference.name + "\n   Barycenter: " + sbBarycenter.name + "\n      Primary: " + sbPrimary.name + "\n    Secondary: " + sbSecondary.name);

            }
        }

        public string OrbitPatcher(Body body)
        {
            // This if is here to make sure stars don't give us trouble
            if (body.generatedBody.orbitDriver.orbit.referenceBody == null)
                body.generatedBody.orbitDriver.orbit.referenceBody = ListOfBodies.Find(rb => rb.name == body.orbit.referenceBody).generatedBody.celestialBody;

            if (!body.generatedBody.celestialBody.Has("sbPatched") && body.generatedBody.celestialBody.Has("orbitPatches"))
            {
                ConfigNode patch = new ConfigNode();
                if (body.generatedBody.celestialBody.orbit != null)
                {
                    OrbitLoader loader = new OrbitLoader();
                    patch.AddData(body.generatedBody.celestialBody.Get<ConfigNode>("orbitPatches"));

                    Parser.LoadObjectFromConfigurationNode(loader, patch);
                    body.generatedBody.orbitDriver.orbit = new Orbit(loader.orbit);
                }
                // This "else" is here to make sure stars don't give us trouble
                else
                {
                    OrbitLoader loader = new OrbitLoader();
                    loader.orbit = new Orbit();
                    loader.orbit.referenceBody = body.generatedBody.orbitDriver.orbit.referenceBody;
                    patch.AddData(body.generatedBody.celestialBody.Get<ConfigNode>("orbitPatches"));

                    Parser.LoadObjectFromConfigurationNode(loader, patch);
                    if (!patch.HasValue("inclination")) loader.orbit.inclination = 0;
                    if (!patch.HasValue("eccentricity")) loader.orbit.eccentricity = 0;
                    if (!patch.HasValue("semiMajorAxis")) loader.orbit.semiMajorAxis = 0;
                    if (!patch.HasValue("longitudeOfAscendingNode")) loader.orbit.LAN = 0;
                    if (!patch.HasValue("argumentOfPeriapsis")) loader.orbit.argumentOfPeriapsis = 0;
                    if (!patch.HasValue("meanAnomalyAtEpoch") && !patch.HasValue("meanAnomalyAtEpochD")) loader.orbit.meanAnomalyAtEpoch = 0;
                    if (!patch.HasValue("epoch")) loader.orbit.epoch = 0;

                    body.generatedBody.orbitDriver.orbit = new Orbit(loader.orbit);
                }


                body.generatedBody.celestialBody.Set("orbitPatches", new ConfigNode());


                if (patch.GetValue("referenceBody") != null)
                    body.orbit.referenceBody = patch.GetValue("referenceBody");

                if (patch.GetValue("referenceBody") != null && body.name == "Kerbin")
                {
                    // Keep the ConfigNode for Kerbin's referenceBody in case Kerbin is the sbSecondary
                    ConfigNode temp = body.generatedBody.celestialBody.Get<ConfigNode>("orbitPatches");
                    temp.AddValue("referenceBody", patch.GetValue("referenceBody"));
                    body.generatedBody.celestialBody.Set("orbitPatches", temp);
                    body.generatedBody.celestialBody.Set("sbPatched", true);
                }
                else
                {
                    body.generatedBody.celestialBody.Set("orbitPatches", new ConfigNode());
                }

                // Fix sphereOfInfluence
                if (!body.generatedBody.celestialBody.Has("sphereOfInfluence"))
                    body.generatedBody.celestialBody.sphereOfInfluence = body.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(body.generatedBody.celestialBody.Mass / ListOfBodies.Find(rb => rb.name == body.orbit.referenceBody).generatedBody.celestialBody.Mass, 0.4);

            }
            return body.orbit.referenceBody;
        }

        public SigmaBinary()
        {
        }
    }
}
