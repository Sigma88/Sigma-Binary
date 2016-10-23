using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;



namespace SigmaBinaryPlugin
{
    [ExternalParserTarget("Properties")]
    public class SigmaBinary : ExternalParserTargetLoader, IParserEventSubscriber
    {

        public static List<Body> ListOfBodies = new List<Body>();
        public static List<Body> ListOfBinaries = new List<Body>();

        public static List<string> ArchivesFixerList = new List<string>();
        public static Dictionary<string, double> periodFixerList = new Dictionary<string, double>();

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

                Body sbPrimary = ListOfBodies.Find(b1 => b1.name == sbSecondary.orbit.referenceBody);
                Body sbBarycenter = ListOfBodies.Find(b0 => b0.name == sigmabinarySBName[sbSecondary]);
                Body sbReference = ListOfBodies.Find(rb => rb.name == sbPrimary.orbit.referenceBody);
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

                ArchivesFixerList.Add(sbBarycenter.name);
                sbBarycenter.generatedBody.orbitDriver.orbit = new Orbit(sbPrimary.generatedBody.orbitDriver.orbit);
                sbBarycenter.orbit.referenceBody = sbPrimary.orbit.referenceBody;
                sbBarycenter.generatedBody.celestialBody.GeeASL = (sbPrimary.generatedBody.celestialBody.Mass + sbSecondary.generatedBody.celestialBody.Mass) /1e5* 6.674e-11d / Math.Pow(sbBarycenter.generatedBody.celestialBody.Radius, 2) / 9.80665d;
                sbBarycenter.generatedBody.celestialBody.rotationPeriod = 0;
                
                if (periodFixerList.ContainsKey(sbBarycenter.name))
                    periodFixerList.Remove(sbBarycenter.name);
                periodFixerList.Add(sbPrimary.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / sbReference.generatedBody.celestialBody.Mass));
                

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
                    
                Debug.Log("SigmaBinaryLog: Description <OK>");

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

                if (sbPrimary.generatedBody.celestialBody.tidallyLocked)
                    sbPrimary.generatedBody.celestialBody.rotationPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.67408E-11 / sbPrimary.generatedBody.celestialBody.Mass);
                sbPrimary.generatedBody.celestialBody.tidallyLocked = false;
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

                if (sigmabinaryPrimaryLocked.ContainsKey(sbSecondary))
                {
                    sbPrimary.generatedBody.celestialBody.rotationPeriod = periodFixerList[sbBarycenter.name];
                }
                



                /// Set Secondary Orbit
                if (sigmabinaryRedrawOrbit.Contains(sbSecondary) && sbOrbit != null)
                {
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



                    if (sbSecondary.generatedBody.celestialBody.GetComponent<NameChanger>())
                    {
                        if (Kopernicus.Templates.drawMode.ContainsKey(sbSecondary.generatedBody.celestialBody.GetComponent<NameChanger>().oldName))
                            Kopernicus.Templates.drawMode.Remove(sbSecondary.generatedBody.celestialBody.GetComponent<NameChanger>().oldName);
                        Kopernicus.Templates.drawMode.Add(sbSecondary.generatedBody.celestialBody.GetComponent<NameChanger>().oldName, OrbitRenderer.DrawMode.OFF);
                    }
                    else
                    {
                        if (Kopernicus.Templates.drawMode.ContainsKey(sbSecondary.name))
                            Kopernicus.Templates.drawMode.Remove(sbSecondary.name);
                        Kopernicus.Templates.drawMode.Add(sbSecondary.name, OrbitRenderer.DrawMode.OFF);
                    }
                }
                



                /// Set SphereOfInfluence for Barycenter and Primary

                if (!Kopernicus.Templates.sphereOfInfluence.ContainsKey(sbPrimary.name))
                {
                    Kopernicus.Templates.sphereOfInfluence.Add(sbPrimary.name, sbBarycenter.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow(sbPrimary.generatedBody.celestialBody.Mass / sbReference.generatedBody.celestialBody.Mass, 0.4));
                }
                Kopernicus.Templates.sphereOfInfluence.Add(sbBarycenter.name, /*sbBarycenter.generatedBody.orbitDriver.orbit.semiMajorAxis * Math.Pow((sbPrimary.generatedBody.celestialBody.Mass + sbSecondary.generatedBody.celestialBody.Mass) / sbReference.generatedBody.celestialBody.Mass, 0.4)); //*/Kopernicus.Templates.sphereOfInfluence[sbPrimary.name]);
                Kopernicus.Templates.sphereOfInfluence[sbPrimary.name] = sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis * (sbBarycenter.generatedBody.orbitDriver.orbit.eccentricity + 1) + Kopernicus.Templates.sphereOfInfluence[sbBarycenter.name];
                
                



                /// Binary System Completed

                ListOfBinaries.Remove(sbSecondary);

                // Log
                Debug.Log("--- BINARY SYSTEM LOADED ---\nReferenceBody: " + sbReference.name + "\n   Barycenter: " + sbBarycenter.name + "\n      Primary: " + sbPrimary.name + "\n    Secondary: " + sbSecondary.name);
            }
        }
        public static int FindClosestPoitsReverted(Orbit p, Orbit s, ref double CD, ref double CCD, ref double FFp, ref double FFs, ref double SFp, ref double SFs, double epsilon, int maxIterations, ref int iterationCount)
        {
            Orbit.FindClosestPoints_old(p, s, ref CD, ref CCD, ref FFp, ref FFs, ref SFp, ref SFs, epsilon, maxIterations, ref iterationCount);
            return 2;
        }

        public SigmaBinary()
        {
        }
    }
}
