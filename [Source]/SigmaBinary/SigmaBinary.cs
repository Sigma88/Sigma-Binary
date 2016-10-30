using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;
using Kopernicus.OnDemand;



namespace SigmaBinaryPlugin
{
    [ExternalParserTarget("Properties")]
    public class SigmaBinary : ExternalParserTargetLoader, IParserEventSubscriber
    {

        public static List<Body> ListOfBodies = new List<Body>();
        public static List<Body> ListOfBinaries = new List<Body>();

        public static Dictionary<string, float> archivesFixerList = new Dictionary<string, float>();
        public static Dictionary<string, double> periodFixerList = new Dictionary<string, double>();
        public static Dictionary<string, string> mapViewFixerList = new Dictionary<string, string>();
        public static string kerbinFixer;

        public static Dictionary<string, Body> sigmabinaryLoadAfter = new Dictionary<string, Body>();
        public static Dictionary<Body, string> sigmabinarySBName = new Dictionary<Body, string>();
        public static Dictionary<Body, bool> sigmabinaryPrimaryLocked = new Dictionary<Body, bool>();
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

            for (int loop = 0; ListOfBinaries.Count > 0; loop++)
            {
                
                /// Loading the Bodies

                Body sbSecondary = ListOfBinaries.First();
                Body sbPrimary = ListOfBodies.Find(b1 => b1.name == OrbitPatcher(sbSecondary));
                Body sbBarycenter = ListOfBodies.Find(b0 => b0.name == sigmabinarySBName[sbSecondary]);
                Body sbReference = ListOfBodies.Find(rb => rb.name == OrbitPatcher(sbPrimary));
                Body sbOrbit = ListOfBodies.Find(ob => ob.name == sigmabinarySBName[sbSecondary] + "Orbit" && sigmabinaryRedrawOrbit.Contains(sbSecondary));


                if (sbPrimary == null || sbBarycenter == null || sbReference == null)
                    break;
                if (sbOrbit == null && sigmabinaryRedrawOrbit.Contains(sbSecondary))
                    break;
                



                /// Generating Binary System


                // Remove Finalize Orbit

                if (Kopernicus.Templates.finalizeBodies.Contains(sbSecondary.name))
                {
                    Kopernicus.Templates.finalizeBodies.Remove(sbSecondary.name);
                    // Fix sphereOfInfluence
                    if (!Kopernicus.Templates.sphereOfInfluence.ContainsKey(sbSecondary.name))
                    {
                        sbSecondary.generatedBody.celestialBody.sphereOfInfluence = Math.Max(sbSecondary.orbit.semiMajorAxis * Math.Pow(sbSecondary.generatedBody.celestialBody.Mass / sbPrimary.generatedBody.celestialBody.Mass, 0.4), Math.Max(sbSecondary.generatedBody.celestialBody.Radius * Kopernicus.Templates.SOIMinRadiusMult, sbSecondary.generatedBody.celestialBody.Radius + Kopernicus.Templates.SOIMinAltitude));
                    }
                }
                if (Kopernicus.Templates.finalizeBodies.Contains(sbPrimary.name))
                {
                    Kopernicus.Templates.finalizeBodies.Remove(sbPrimary.name);
                    // Fix sphereOfInfluence
                    if (!Kopernicus.Templates.sphereOfInfluence.ContainsKey(sbPrimary.name))
                    {
                        sbPrimary.generatedBody.celestialBody.sphereOfInfluence = Math.Max(sbPrimary.orbit.semiMajorAxis * Math.Pow(sbPrimary.generatedBody.celestialBody.Mass / sbReference.generatedBody.celestialBody.Mass, 0.4), Math.Max(sbPrimary.generatedBody.celestialBody.Radius * Kopernicus.Templates.SOIMinRadiusMult, sbPrimary.generatedBody.celestialBody.Radius + Kopernicus.Templates.SOIMinAltitude));
                    }
                }

                


                /// Set Barycenter

                sbBarycenter.generatedBody.orbitDriver.orbit = new Orbit(sbPrimary.generatedBody.orbitDriver.orbit);
                sbBarycenter.orbit.referenceBody = sbPrimary.orbit.referenceBody;
                sbBarycenter.generatedBody.celestialBody.GeeASL = (sbPrimary.generatedBody.celestialBody.Mass + sbSecondary.generatedBody.celestialBody.Mass) /1e5* 6.674e-11d / Math.Pow(sbBarycenter.generatedBody.celestialBody.Radius, 2) / 9.80665d;
                sbBarycenter.generatedBody.celestialBody.rotationPeriod = 0;
                
                if (periodFixerList.ContainsKey(sbBarycenter.name))
                    periodFixerList.Remove(sbBarycenter.name);
                periodFixerList.Add(sbBarycenter.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / sbReference.generatedBody.celestialBody.Mass));
                

                // Orbit Color

                if (sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                    sbBarycenter.generatedBody.orbitRenderer.SetColor(sigmabinaryOrbitColor[sbSecondary]);
                else
                    sbBarycenter.generatedBody.orbitRenderer.orbitColor = sbPrimary.generatedBody.orbitRenderer.orbitColor;
                

                // Icon Color

                if (sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                    sbBarycenter.generatedBody.orbitRenderer.nodeColor = sigmabinaryIconColor[sbSecondary];
                else if (!sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                    sbBarycenter.generatedBody.orbitRenderer.nodeColor = sbPrimary.generatedBody.orbitRenderer.nodeColor;
                

                // Description

                if (!sigmabinaryDescription.ContainsKey(sbSecondary))
                {
                    sbBarycenter.generatedBody.celestialBody.bodyDescription = "This is the barycenter of the ";
                    if (sbPrimary.generatedBody.celestialBody.GetComponent<NameChanger>() != null)
                        sbBarycenter.generatedBody.celestialBody.bodyDescription = sbBarycenter.generatedBody.celestialBody.bodyDescription + sbPrimary.generatedBody.celestialBody.GetComponent<NameChanger>().newName;
                    else
                        sbBarycenter.generatedBody.celestialBody.bodyDescription = sbBarycenter.generatedBody.celestialBody.bodyDescription + sbPrimary.name;
                    if (sbSecondary.generatedBody.celestialBody.GetComponent<NameChanger>() != null)
                        sbBarycenter.generatedBody.celestialBody.bodyDescription = sbBarycenter.generatedBody.celestialBody.bodyDescription + "-" + sbSecondary.generatedBody.celestialBody.GetComponent<NameChanger>().newName + " system.";
                    else
                        sbBarycenter.generatedBody.celestialBody.bodyDescription = sbBarycenter.generatedBody.celestialBody.bodyDescription + "-" + sbSecondary.generatedBody.celestialBody.name + " system.";
                }
                else
                    sbBarycenter.generatedBody.celestialBody.bodyDescription = sigmabinaryDescription[sbSecondary];
                

                // DrawMode and DrawIcons

                if (Kopernicus.Templates.drawMode.ContainsKey(sbBarycenter.name))
                    Kopernicus.Templates.drawMode.Remove(sbBarycenter.name);
                if (Kopernicus.Templates.drawIcons.ContainsKey(sbBarycenter.name))
                    Kopernicus.Templates.drawIcons.Remove(sbBarycenter.name);
                if (sigmabinaryMode.ContainsKey(sbSecondary))
                    Kopernicus.Templates.drawMode.Add(sbBarycenter.name, sigmabinaryMode[sbSecondary]);
                if (sigmabinaryIcon.ContainsKey(sbSecondary))
                    Kopernicus.Templates.drawIcons.Add(sbBarycenter.name, sigmabinaryIcon[sbSecondary]);

                


                /// Set Primary

                archivesFixerList.Add(sbPrimary.name, 0f);
                sbPrimary.generatedBody.orbitDriver.orbit =
                    new Orbit
                    (
                        sbSecondary.generatedBody.orbitDriver.orbit.inclination,
                        sbSecondary.generatedBody.orbitDriver.orbit.eccentricity,
                        sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis * sbSecondary.generatedBody.celestialBody.Mass / (sbSecondary.generatedBody.celestialBody.Mass + sbPrimary.generatedBody.celestialBody.Mass),
                        sbSecondary.generatedBody.orbitDriver.orbit.LAN,
                        sbSecondary.generatedBody.orbitDriver.orbit.argumentOfPeriapsis + 180d,
                        sbSecondary.generatedBody.orbitDriver.orbit.meanAnomalyAtEpoch,
                        sbSecondary.generatedBody.orbitDriver.orbit.epoch,
                        sbBarycenter.generatedBody.celestialBody
                    );
                sbPrimary.orbit.referenceBody = sbBarycenter.name;

                if (periodFixerList.ContainsKey(sbPrimary.name))
                    periodFixerList.Remove(sbPrimary.name);
                periodFixerList.Add(sbPrimary.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / sbPrimary.generatedBody.celestialBody.Mass));
                

                if (Kopernicus.Templates.drawMode.ContainsKey(sbPrimary.name))
                    Kopernicus.Templates.drawMode.Remove(sbPrimary.name);
                if (Kopernicus.Templates.drawIcons.ContainsKey(sbPrimary.name))
                    Kopernicus.Templates.drawIcons.Remove(sbPrimary.name);
                

                // Primary Locked

                if (sbPrimary.generatedBody.celestialBody.solarRotationPeriod)
                {
                    sbPrimary.generatedBody.celestialBody.solarRotationPeriod = false;
                    sbPrimary.generatedBody.celestialBody.rotationPeriod = (periodFixerList[sbBarycenter.name] * sbPrimary.generatedBody.celestialBody.rotationPeriod) / (periodFixerList[sbBarycenter.name] + sbPrimary.generatedBody.celestialBody.rotationPeriod);
                }
                if (sigmabinaryPrimaryLocked.ContainsKey(sbSecondary))
                {
                    sbPrimary.generatedBody.celestialBody.solarRotationPeriod = false;
                    sbPrimary.generatedBody.celestialBody.rotationPeriod = periodFixerList[sbPrimary.name];
                }


                

                /// Set Secondary Orbit

                if (sigmabinaryRedrawOrbit.Contains(sbSecondary) && sbOrbit != null)
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
                            sbBarycenter.generatedBody.celestialBody
                        );
                    sbOrbit.orbit.referenceBody = sbBarycenter.name;
                    sbOrbit.generatedBody.orbitRenderer.orbitColor = sbSecondary.generatedBody.orbitRenderer.orbitColor;


                    if (periodFixerList.ContainsKey(sbOrbit.name))
                        periodFixerList.Remove(sbOrbit.name);
                    periodFixerList.Add(sbOrbit.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / sbPrimary.generatedBody.celestialBody.Mass));
                    

                    if (Kopernicus.Templates.drawMode.ContainsKey(sbSecondary.generatedBody.transform.name))
                        Kopernicus.Templates.drawMode.Remove(sbSecondary.generatedBody.transform.name);
                    Kopernicus.Templates.drawMode.Add(sbSecondary.generatedBody.transform.name, OrbitRenderer.DrawMode.OFF);
                    
                }

                


                /// Set SphereOfInfluence for Barycenter and Primary

                if (!Kopernicus.Templates.sphereOfInfluence.ContainsKey(sbPrimary.name))
                {
                    Kopernicus.Templates.sphereOfInfluence.Add(sbPrimary.name, sbBarycenter.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(sbPrimary.generatedBody.celestialBody.Mass / sbReference.generatedBody.celestialBody.Mass, 0.4));
                }
                Kopernicus.Templates.sphereOfInfluence.Add(sbBarycenter.name, Kopernicus.Templates.sphereOfInfluence[sbPrimary.name]);
                Kopernicus.Templates.sphereOfInfluence[sbPrimary.name] = sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis * (sbBarycenter.generatedBody.orbitDriver.orbit.eccentricity + 1) + Kopernicus.Templates.sphereOfInfluence[sbBarycenter.name];

                
                if (sbPrimary.name == "Kerbin")
                {
                    // Bypass PostSpawnOrbit
                    Kopernicus.Templates.orbitPatches.Remove("Kerbin");
                    kerbinFixer = sbPrimary.orbit.referenceBody;
                    sbPrimary.orbit.referenceBody = "Sun";
                }
                if (sbSecondary.name == "Kerbin")
                {
                    // Let Kopernicus handle this with PostSpawnOrbit
                    sbPrimary.orbit.referenceBody = "Sun";
                }

                


                /// Binary System Completed

                ListOfBinaries.Remove(sbSecondary);
                TextureFixer(sbPrimary, sbSecondary);

                // Log
                Debug.Log("\nSigmaBinaryLog:\n\n--- BINARY SYSTEM LOADED ---\nReferenceBody: " + sbReference.name + "\n   Barycenter: " + sbBarycenter.name + "\n      Primary: " + sbPrimary.name + "\n    Secondary: " + sbSecondary.name);

            }
        }

        public static int FindClosestPointsReverted(Orbit p, Orbit s, ref double CD, ref double CCD, ref double FFp, ref double FFs, ref double SFp, ref double SFs, double epsilon, int maxIterations, ref int iterationCount)
        {
            Orbit.FindClosestPoints_old(p, s, ref CD, ref CCD, ref FFp, ref FFs, ref SFp, ref SFs, epsilon, maxIterations, ref iterationCount);
            return 2;
        }

        public static string OrbitPatcher(Body body)
        {

            if (body.generatedBody.orbitDriver.orbit.referenceBody == null)
                body.generatedBody.orbitDriver.orbit.referenceBody = ListOfBodies.Find(rb => rb.name == body.orbit.referenceBody).generatedBody.celestialBody;

            if (Kopernicus.Templates.orbitPatches.ContainsKey(body.name) && Kopernicus.Templates.orbitPatches[body.name].GetValue("sbPatched") != "true")
            {
                ConfigNode patch = new ConfigNode();
                if (body.generatedBody.celestialBody.orbit != null)
                {
                    OrbitLoader loader = new OrbitLoader(body.generatedBody.celestialBody);
                    patch.AddData(Kopernicus.Templates.orbitPatches[body.name]);
                    
                    Parser.LoadObjectFromConfigurationNode(loader, patch);
                    body.generatedBody.celestialBody.orbitDriver.orbit = new Orbit(loader.orbit);
                }
                else
                {
                    OrbitLoader loader = new OrbitLoader();
                    loader.orbit = new Orbit();
                    loader.orbit.referenceBody = body.generatedBody.orbitDriver.orbit.referenceBody;
                    patch.AddData(Kopernicus.Templates.orbitPatches[body.name]);

                    
                    Parser.LoadObjectFromConfigurationNode(loader, patch);
                    if (!patch.HasValue("inclination")) loader.orbit.inclination = 0;
                    if (!patch.HasValue("eccentricity")) loader.orbit.eccentricity = 0;
                    if (!patch.HasValue("semiMajorAxis")) loader.orbit.semiMajorAxis = 0;
                    if (!patch.HasValue("longitudeOfAscendingNode")) loader.orbit.LAN = 0;
                    if (!patch.HasValue("argumentOfPeriapsis")) loader.orbit.argumentOfPeriapsis = 0;
                    if (!patch.HasValue("meanAnomalyAtEpoch") && !patch.HasValue("meanAnomalyAtEpochD")) loader.orbit.meanAnomalyAtEpoch = 0;
                    if (!patch.HasValue("epoch")) loader.orbit.epoch = 0;
                    

                    body.generatedBody.celestialBody.orbitDriver = new OrbitDriver();
                    body.generatedBody.celestialBody.orbitDriver.orbit = new Orbit(loader.orbit);
                }
                

                Kopernicus.Templates.orbitPatches[body.name].ClearValues();
                

                if (patch.GetValue("referenceBody") != null)
                    body.orbit.referenceBody = patch.GetValue("referenceBody");

                if (patch.GetValue("referenceBody") != null && body.name == "Kerbin")
                {
                    // Keep the ConfigNode for Kerbin's referenceBody in case Kerbin is the sbSecondary
                    Kopernicus.Templates.orbitPatches[body.name].AddValue("referenceBody", patch.GetValue("referenceBody"));
                    Kopernicus.Templates.orbitPatches[body.name].AddValue("sbPatched", "true");
                }
                else
                {
                    Kopernicus.Templates.orbitPatches.Remove(body.name);
                }
                
                // Fix sphereOfInfluence
                if (!Kopernicus.Templates.sphereOfInfluence.ContainsKey(body.name))
                    body.generatedBody.celestialBody.sphereOfInfluence = body.generatedBody.celestialBody.orbit.semiMajorAxis * Math.Pow(body.generatedBody.celestialBody.Mass / ListOfBodies.Find(rb => rb.name == body.orbit.referenceBody).generatedBody.celestialBody.Mass, 0.4);
                
            }
            return body.orbit.referenceBody;
        }

        public static int TextureFixer(Body body1, Body body2)
        {
            if (DateTime.Today.Day == 14 && DateTime.Today.Month == 2)
            {
                Texture2D MainTex = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == "NewMunSurfaceMapDiffuse").FirstOrDefault() as Texture2D;
                Texture2D BumpMap = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == "NewMunSurfaceMapNormals").FirstOrDefault() as Texture2D;

                foreach (Body b in ListOfBodies)
                {
                    EnumParser<BodyType> type = new EnumParser<BodyType>(b.template == null ? BodyType.Atmospheric : b.template.type);

                    if (type != BodyType.Star)
                    {
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_MainTex", MainTex);
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_BumpMap", BumpMap);

                        if (OnDemandStorage.useOnDemand)
                        {
                            ScaledSpaceDemand demand = b.generatedBody.scaledVersion.GetComponent<ScaledSpaceDemand>();
                            demand.texture = MainTex.name;
                            demand.normals = BumpMap.name;
                        }

                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2(0, 0));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_BumpMap", new Vector2(1, 1));
                    }
                }
            }
            if (DateTime.Today.Day == 1 && DateTime.Today.Month == 4)
            {
                EnumParser<BodyType> type1 = new EnumParser<BodyType>(body1.template == null ? BodyType.Atmospheric : body1.template.type);
                EnumParser<BodyType> type2 = new EnumParser<BodyType>(body2.template == null ? BodyType.Atmospheric : body2.template.type);

                if (type1.value != BodyType.Star && type2.value != BodyType.Star)
                {
                    Material material1 = new Material(body1.generatedBody.scaledVersion.GetComponent<Renderer>().material);
                    Material material2 = new Material(body2.generatedBody.scaledVersion.GetComponent<Renderer>().material);
                    body1.generatedBody.scaledVersion.GetComponent<Renderer>().material = material2;
                    body2.generatedBody.scaledVersion.GetComponent<Renderer>().material = material1;

                    if (OnDemandStorage.useOnDemand)
                    {
                        ScaledSpaceDemand demand1 = body1.generatedBody.scaledVersion.GetComponent<ScaledSpaceDemand>();
                        ScaledSpaceDemand demand2 = body2.generatedBody.scaledVersion.GetComponent<ScaledSpaceDemand>();
                        demand1.texture = material2.GetTexture("_MainTex").name;
                        demand1.normals = material2.GetTexture("_BumpMap").name;
                        demand2.texture = material1.GetTexture("_MainTex").name;
                        demand2.normals = material1.GetTexture("_BumpMap").name;
                    }
                }
            }
            if (DateTime.Today.Day == 22 && DateTime.Today.Month == 4)
            {
                Texture2D MainTex = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == "KerbinScaledSpace300").FirstOrDefault() as Texture2D;
                Texture2D BumpMap = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == "KerbinScaledSpace401").FirstOrDefault() as Texture2D;

                foreach (Body b in ListOfBodies)
                {
                    EnumParser<BodyType> type = new EnumParser<BodyType>(b.template == null ? BodyType.Atmospheric : b.template.type);

                    if (type != BodyType.Star)
                    {
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_MainTex", MainTex);
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_BumpMap", BumpMap);

                        if (OnDemandStorage.useOnDemand)
                        {
                            ScaledSpaceDemand demand = b.generatedBody.scaledVersion.GetComponent<ScaledSpaceDemand>();
                            demand.texture = MainTex.name;
                            demand.normals = BumpMap.name;
                        }

                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2(0, 0));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_BumpMap", new Vector2(1, 1));
                    }
                }
            }
            if (DateTime.Today.Day == 25 && DateTime.Today.Month == 05)
            {
                ListOfBodies.Find(x => x.name == "Sun").generatedBody.celestialBody.bodyDescription = "\n\n\n                        DON'T\n                        PANIC";
                ListOfBodies.Find(x => x.name == "Kerbin").generatedBody.celestialBody.bodyDescription = "Mostly harmless.";
            }
            if (DateTime.Today.Day == 31 && DateTime.Today.Month == 10)
            {
                foreach (Body b in ListOfBodies)
                {
                    if (b.generatedBody.orbitRenderer != null)
                    {
                        b.generatedBody.orbitRenderer.SetColor(new Color(0.5f, 0.25f, 0f, 1f));
                    }
                }
            }
            return 1;
        }

        public SigmaBinary()
        {
        }
    }
}
