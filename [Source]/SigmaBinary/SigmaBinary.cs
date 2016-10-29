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

            foreach (Body sbSecondary in ListOfBinaries)
            {

                /// Loading the Bodies

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
            if (Kopernicus.Templates.orbitPatches.ContainsKey(body.name) && Kopernicus.Templates.orbitPatches[body.name].GetValue("sbPatched") != "true")
            {
                OrbitLoader loader = new OrbitLoader(body.generatedBody.celestialBody);
                ConfigNode patch = new ConfigNode();
                patch.AddData(Kopernicus.Templates.orbitPatches[body.name]);


                Parser.LoadObjectFromConfigurationNode(loader, patch);
                body.generatedBody.celestialBody.orbitDriver.orbit = new Orbit(loader.orbit);
                

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
            Debug.Log("SigmaBinaryLog: 1");
            Texture texture1 = body1.generatedBody.scaledVersion.GetComponent<Renderer>().material.GetTexture("_MainTex");
            Texture texture2 = body2.generatedBody.scaledVersion.GetComponent<Renderer>().material.GetTexture("_MainTex");
            body1.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_MainTex", texture2);
            body2.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_MainTex", texture1);


            /*
            Texture2D normals1 = body1.generatedBody.scaledVersion.GetComponent<Renderer>().material.GetTexture("_BumpMap") as Texture2D;
            ScaledSpaceDemand demand2 = body1.generatedBody.scaledVersion.AddComponent<ScaledSpaceDemand>();
            Texture2D texture2 = body2.generatedBody.scaledVersion.GetComponent<Renderer>().material.GetTexture("_MainTex") as Texture2D;
            Texture2D normals2 = body2.generatedBody.scaledVersion.GetComponent<Renderer>().material.GetTexture("_BumpMap") as Texture2D;
            ScaledSpaceDemand demand1 = body2.generatedBody.scaledVersion.AddComponent<ScaledSpaceDemand>();
            Debug.Log("SigmaBinaryLog: 2");
            if (texture1 != null)
                demand1.texture = texture1.name;
            if (normals1 != null)
                demand1.normals = normals1.name;
            if (texture2 != null)
                demand2.texture = texture2.name;
            if (normals2 != null)
                demand2.normals = normals2.name;
            Debug.Log("SigmaBinaryLog: demand1.texture = " + demand1.texture);
            Debug.Log("SigmaBinaryLog: demand1.normals = " + demand1.normals);
            Debug.Log("SigmaBinaryLog: demand2.texture = " + demand2.texture);
            Debug.Log("SigmaBinaryLog: demand2.normals = " + demand2.normals);*/
            return 1;
        }

        public SigmaBinary()
        {
        }
    }
}
