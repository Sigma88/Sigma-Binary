using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;

using SigmaBinaryPlugin.Components;


namespace SigmaBinaryPlugin
{
    namespace Configuration
    {
        [ExternalParserTarget("SigmaBinary")]
        public class SigmaBinaryLoader : ExternalParserTargetLoader, IParserEventSubscriber
        {
            public SigmaBinary sigmabinary { get; set; }
            public static Dictionary<string, CelestialBody> sigmabinaryLoadAfter = new Dictionary<string, CelestialBody>();
            public static Dictionary<string, string> sigmabinaryName = new Dictionary<string, string>();
            public static Dictionary<string, bool> sigmabinaryPrimaryLocked = new Dictionary<string, bool>();
            public static Dictionary<string, bool> sigmabinaryRedrawOrbit = new Dictionary<string, bool>();
            public static Dictionary<string, string> sigmabinaryDescription = new Dictionary<string, string>();
            public static Dictionary<string, bool> sigmabinarySelectable = new Dictionary<string, bool>();
            public static Dictionary<string, Color> sigmabinaryColor = new Dictionary<string, Color>();
            public static Dictionary<string, EnumParser<OrbitRenderer.DrawMode>> sigmabinaryMode = new Dictionary<string, EnumParser<OrbitRenderer.DrawMode>>();
            public static Dictionary<string, EnumParser<OrbitRenderer.DrawIcons>> sigmabinaryIcon = new Dictionary<string, EnumParser<OrbitRenderer.DrawIcons>>();

            [ParserTarget("name", optional = true)]
            public string sbName
            {
                get { return sigmabinary.sbName; }
                set { sigmabinary.sbName = value; }
            }

            [ParserTarget("primaryLocked", optional = true)]
            public NumericParser<bool> primaryLocked
            {
                get { return sigmabinary.primaryLocked; }
                set { sigmabinary.primaryLocked = value; }
            }

            [ParserTarget("after", optional = true)]
            public string after
            {
                get { return sigmabinary.after; }
                set { sigmabinary.after = value; }
            }

            [ParserTarget("redrawOrbit", optional = true)]
            public NumericParser<bool> redrawOrbit
            {
                get { return sigmabinary.redrawOrbit; }
                set { sigmabinary.redrawOrbit = value; }
            }
            
            [ParserTarget("Properties", optional = true, allowMerge = true)]
            public SigmaBinaryPropertiesLoader sigmabinaryproperties { get; set; }

            [ParserTarget("Orbit", optional = true, allowMerge = true)]
            public SigmaBinaryOrbitLoader sigmabinaryorbit { get; set; }
            
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                sigmabinary = generatedBody.celestialBody.gameObject.AddComponent<SigmaBinary>();
            }

            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
                //  Debug.Log("SigmaBinaryLoaderLog: Starting PostApply for generatedBody " + generatedBody.name);
                // Set Fake Orbit Color
                sigmabinaryColor.Add(sigmabinary.sbName + "Orbit", generatedBody.orbitRenderer.orbitColor);
                //  Debug.Log("SigmaBinaryLoaderLog: Added Color for fake orbit - sigmabinaryColor.Add(" + sigmabinary.sbName + "Orbit, " + generatedBody.orbitRenderer.orbitColor + ")");

                // Unless we have a custom color
                if (!sigmabinaryColor.ContainsKey(sigmabinary.sbName))
                {
                    //  Debug.Log("SigmaBinaryLoaderLog: No Custom color detected for barycenter " + sigmabinary.sbName);
                    // Look for Primary Orbit Color
                    if (sigmabinaryColor.ContainsKey(ColorSwitcher.referenceSwitcher[generatedBody]))
                    {
                        //  Debug.Log("SigmaBinaryLoaderLog: Found color of primary body named " + ColorSwitcher.referenceSwitcher[generatedBody]);
                        // If Primary has already been loaded, take the color
                        sigmabinaryColor.Add(sigmabinary.sbName, sigmabinaryColor[ColorSwitcher.referenceSwitcher[generatedBody]]);
                        //  Debug.Log("SigmaBinaryLoaderLog: the selected color is added to the list - sigmabinaryColor.Add(" + sigmabinary.sbName + ", " + sigmabinaryColor[ColorSwitcher.referenceSwitcher[generatedBody]]);
                    }
                    else
                    {
                        //  Debug.Log("SigmaBinaryLoaderLog: No colors stored for primary body named " + ColorSwitcher.referenceSwitcher[generatedBody]);
                        // Otherwise request the color
                        ColorSwitcher.colorSwitcher.Add(ColorSwitcher.referenceSwitcher[generatedBody], sigmabinary.sbName);
                        //  Debug.Log("SigmaBinaryLoaderLog: Requested color for the barycenter - ColorSwitcher.colorSwitcher.Add(" + ColorSwitcher.referenceSwitcher[generatedBody] + ", " + sigmabinary.sbName);
                    }
                }
                else
                {
                    // If we have a custom color we don't need to change anything
                    //  Debug.Log("SigmaBinaryLoaderLog: Found Custom color " + sigmabinaryColor[sigmabinary.sbName].ToString() + " detected for body " + sigmabinary.sbName);
                }

                //  Debug.Log("SigmaBinaryLoaderLog: Ending PostApply for generatedBody " + generatedBody.name);
            }

            public SigmaBinaryLoader()
            {
            }
        }
        public class SigmaBinaryPropertiesLoader : BaseLoader, IParserEventSubscriber
        {
            public SigmaBinary sigmabinary { get; set; }

            // description for the body
            [ParserTarget("description", optional = true)]
            public string description
            {
                get { return sigmabinary.description; }
                set { sigmabinary.description = value; }
            }
            
            // If the body should be unselectable
            [ParserTarget("selectable", optional = true)]
            public NumericParser<bool> selectable
            {
                get { return sigmabinary.selectable; }
                set { sigmabinary.selectable = value; }
            }

            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                sigmabinary = generatedBody.celestialBody.gameObject.GetComponent<SigmaBinary>();
            }

            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }

            public SigmaBinaryPropertiesLoader()
            {
            }
        }
        public class SigmaBinaryOrbitLoader : BaseLoader, IParserEventSubscriber
        {
            public SigmaBinary sigmabinary { get; set; }

            // Orbit Draw Mode
            [ParserTarget("mode", optional = true)]
            public EnumParser<OrbitRenderer.DrawMode> mode
            {
                set { SigmaBinaryLoader.sigmabinaryMode.Add(sigmabinary.sbName, value); }
            }

            // Orbit Icon Mode
            [ParserTarget("icon", optional = true)]
            public EnumParser<OrbitRenderer.DrawIcons> icon
            {
                set { SigmaBinaryLoader.sigmabinaryIcon.Add(sigmabinary.sbName, value); }
            }

            // Orbit Color
            [ParserTarget("color", optional = true)]
            public ColorParser color
            {
                set
                {
                    SigmaBinaryLoader.sigmabinaryColor.Add(sigmabinary.sbName, new Color(value.value.r / 2, value.value.g / 2, value.value.b / 2, value.value.r / 2));
                }
            }

            void IParserEventSubscriber.Apply(ConfigNode node)
            {
                sigmabinary = generatedBody.celestialBody.gameObject.GetComponent<SigmaBinary>();
            }

            void IParserEventSubscriber.PostApply(ConfigNode node)
            {
            }

            public SigmaBinaryOrbitLoader()
            {
            }
        }
    }
}
