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

                Debug.Log("SigmaBinaryLog: Generating system");
                Debug.Log("SigmaBinaryLog: Reference = " + sbReference.name);
                Debug.Log("SigmaBinaryLog: Barycenter = " + sbBarycenter.name);
                Debug.Log("SigmaBinaryLog: Primary = " + sbPrimary.name);
                Debug.Log("SigmaBinaryLog: Secondary = " + sbSecondary.name);
                if (sbOrbit != null)
                    Debug.Log("SigmaBinaryLog: Orbit = " + sbOrbit.name);
                else
                    Debug.Log("SigmaBinaryLog: Orbit = null");


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

                Debug.Log("SigmaBinaryLog: Remove Finalize Orbit <OK>");



                /// Set Barycenter

                ArchivesFixerList.Add(sbBarycenter.name);
                sbBarycenter.generatedBody.orbitDriver.orbit = new Orbit(sbPrimary.generatedBody.orbitDriver.orbit);
                sbBarycenter.orbit.referenceBody = sbPrimary.orbit.referenceBody;
                sbBarycenter.generatedBody.celestialBody.GeeASL = (sbPrimary.generatedBody.celestialBody.Mass / 1e5d) * 6.674e-11d / Math.Pow(sbBarycenter.generatedBody.celestialBody.Radius, 2) / 9.81d;
                sbBarycenter.generatedBody.celestialBody.rotationPeriod = sbSecondary.generatedBody.orbitDriver.orbit.period;

                periodFixerList.Add(sbBarycenter.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.674E-11 / sbReference.generatedBody.celestialBody.Mass));
                

                /*
                sbBarycenter.generatedBody.celestialBody.orbit.period = sbPrimary.generatedBody.celestialBody.orbit.period;
                sbBarycenter.generatedBody.celestialBody.orbit.meanMotion = 2 * Math.PI / sbBarycenter.generatedBody.celestialBody.orbit.period;
                sbBarycenter.generatedBody.celestialBody.orbit.ObTAtEpoch = sbPrimary.generatedBody.celestialBody.orbit.ObTAtEpoch;
                */


                Debug.Log("SigmaBinaryLog: Set Barycenter <OK>");

                // Orbit Color

                if (sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                    sbBarycenter.generatedBody.orbitRenderer.SetColor(sigmabinaryOrbitColor[sbSecondary]);
                else
                    sbBarycenter.generatedBody.orbitRenderer.orbitColor = sbPrimary.generatedBody.orbitRenderer.orbitColor;

                Debug.Log("SigmaBinaryLog: set barycenter orbit color <OK>");

                // Icon Color

                if (sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                    sbBarycenter.generatedBody.orbitRenderer.nodeColor = sigmabinaryIconColor[sbSecondary];
                else if (!sigmabinaryOrbitColor.ContainsKey(sbSecondary))
                    sbBarycenter.generatedBody.orbitRenderer.nodeColor = sbPrimary.generatedBody.orbitRenderer.nodeColor;

                Debug.Log("SigmaBinaryLog: set barycenter icon color <OK>");

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

                Debug.Log("SigmaBinaryLog: DrawMode and DrawIcons <OK>");



                /// Set Primary

                if (sbPrimary.generatedBody.celestialBody.tidallyLocked)
                    sbPrimary.generatedBody.celestialBody.rotationPeriod = sbPrimary.generatedBody.orbitDriver.orbit.period;
                sbPrimary.generatedBody.celestialBody.tidallyLocked = false;

                Debug.Log("SigmaBinaryLog: Primary Orbital Period (OLD) = " + sbPrimary.generatedBody.orbitDriver.orbit.period);
                Debug.Log("SigmaBinaryLog: Primary Orbital Period (OLD) = " + sbPrimary.generatedBody.orbitDriver.orbit.period);
                Debug.Log("SigmaBinaryLog: Primary Orbital Period (OLD) = " + sbPrimary.generatedBody.orbitDriver.orbit.period);
                Debug.Log("SigmaBinaryLog: Primary Orbital Period (OLD) = " + sbPrimary.generatedBody.orbitDriver.orbit.period);
                Debug.Log("SigmaBinaryLog: Barycenter Orbital Period = " + sbBarycenter.generatedBody.orbitDriver.orbit.period);
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


                periodFixerList.Add(sbPrimary.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.674E-11 / sbPrimary.generatedBody.celestialBody.Mass));

                /*
                sbPrimary.generatedBody.orbitDriver.orbit.period = sbSecondary.generatedBody.orbitDriver.orbit.period;
                sbPrimary.generatedBody.orbitDriver.orbit.meanMotion = 2 * Math.PI / sbPrimary.generatedBody.orbitDriver.orbit.period;
                sbPrimary.generatedBody.orbitDriver.orbit.ObTAtEpoch = sbSecondary.generatedBody.orbitDriver.orbit.ObTAtEpoch;
                Debug.Log("SigmaBinaryLog: Primary Orbital Period (NEW) = " + sbPrimary.generatedBody.orbitDriver.orbit.period);
                Debug.Log("SigmaBinaryLog: Secondary Orbital Period = " + sbSecondary.generatedBody.orbitDriver.orbit.period);
                */


                if (Kopernicus.Templates.drawMode.ContainsKey(sbPrimary.name))
                    Kopernicus.Templates.drawMode.Remove(sbPrimary.name);
                if (Kopernicus.Templates.drawIcons.ContainsKey(sbPrimary.name))
                    Kopernicus.Templates.drawIcons.Remove(sbPrimary.name);

                Debug.Log("SigmaBinaryLog: Set Primary <OK>");

                // Primary Locked

                if (sigmabinaryPrimaryLocked.ContainsKey(sbSecondary))
                {
                    sbPrimary.generatedBody.celestialBody.rotationPeriod = sbSecondary.generatedBody.orbitDriver.orbit.period;
                }

                Debug.Log("SigmaBinaryLog: PrimaryLocked <OK>");



                /// Set Secondary Orbit
                if (sigmabinaryRedrawOrbit.Contains(sbSecondary) && sbOrbit != null)
                {
                    sbOrbit.generatedBody.orbitDriver.orbit = 
                        new Orbit
                        (
                            sbSecondary.generatedBody.orbitDriver.orbit.inclination,
                            sbSecondary.generatedBody.orbitDriver.orbit.eccentricity,
                            sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis - sbPrimary.orbit.semiMajorAxis,
                            sbSecondary.generatedBody.orbitDriver.orbit.LAN,
                            sbSecondary.generatedBody.orbitDriver.orbit.argumentOfPeriapsis,
                            sbSecondary.generatedBody.orbitDriver.orbit.meanAnomalyAtEpoch,
                            sbSecondary.generatedBody.orbitDriver.orbit.epoch,
                            sbBarycenter.generatedBody.celestialBody
                        );
                    sbOrbit.orbit.referenceBody = sbBarycenter.name;
                    sbOrbit.generatedBody.orbitRenderer.orbitColor = sbSecondary.generatedBody.orbitRenderer.orbitColor;


                    periodFixerList.Add(sbOrbit.name, 2 * Math.PI * Math.Sqrt(Math.Pow(sbSecondary.generatedBody.orbitDriver.orbit.semiMajorAxis, 3) / 6.674E-11 / sbPrimary.generatedBody.celestialBody.Mass));


                    /*
                    sbOrbit.generatedBody.orbitDriver.orbit.period = sbSecondary.generatedBody.orbitDriver.orbit.period;
                    sbOrbit.generatedBody.orbitDriver.orbit.meanMotion = 2 * Math.PI / sbOrbit.generatedBody.orbitDriver.orbit.period;
                    sbOrbit.generatedBody.orbitDriver.orbit.ObTAtEpoch = sbSecondary.generatedBody.orbitDriver.orbit.ObTAtEpoch;
                    */


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

                Debug.Log("SigmaBinaryLog: Set Fake Orbit <OK>");



                /// Set SphereOfInfluence for Barycenter and Primary

                if (Kopernicus.Templates.sphereOfInfluence.ContainsKey(sbPrimary.name))
                {
                    sbPrimary.generatedBody.celestialBody.sphereOfInfluence = Kopernicus.Templates.sphereOfInfluence[sbPrimary.name];
                    Kopernicus.Templates.sphereOfInfluence.Remove(sbPrimary.name);
                }
                sbBarycenter.generatedBody.celestialBody.sphereOfInfluence = sbPrimary.generatedBody.celestialBody.sphereOfInfluence;
                Kopernicus.Templates.sphereOfInfluence.Add(sbBarycenter.name, sbBarycenter.generatedBody.celestialBody.sphereOfInfluence);

                sbPrimary.generatedBody.celestialBody.sphereOfInfluence = sbPrimary.generatedBody.orbitDriver.orbit.semiMajorAxis * (sbPrimary.generatedBody.orbitDriver.orbit.eccentricity + 1) + sbBarycenter.generatedBody.celestialBody.sphereOfInfluence;
                Kopernicus.Templates.sphereOfInfluence.Add(sbPrimary.name, sbPrimary.generatedBody.celestialBody.sphereOfInfluence);

                Debug.Log("SigmaBinaryLog: Set SoI for sbB and sbP <OK>");







                /// Binary System Completed

                ListOfBinaries.Remove(sbSecondary);

                // Log
                Debug.Log("--- BINARY SYSTEM LOADED ---\nReferenceBody: " + sbReference.name + "\n   Barycenter: " + sbBarycenter.name + "\n      Primary: " + sbPrimary.name + "\n    Secondary: " + sbSecondary.name);
            }
        }


        public SigmaBinary()
        {
        }
    }
}
