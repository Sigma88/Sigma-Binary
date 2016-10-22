using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;


namespace SigmaBinaryPlugin
{
    namespace Configuration
    {
        [ExternalParserTarget("Orbit")]
        public class ColorSwitcher : ExternalParserTargetLoader, IParserEventSubscriber
        {
            public static Dictionary<string, string> colorSwitcher = new Dictionary<string, string>();
            public static Dictionary<PSystemBody, string> referenceSwitcher = new Dictionary<PSystemBody, string>();

            [ParserTarget("referenceBody", optional = true)]
            public string referenceBody
            {
                set { referenceSwitcher.Add(generatedBody, value); }
            }
            void IParserEventSubscriber.Apply(ConfigNode node)
            {
            }

            void IParserEventSubscriber.PostApply(ConfigNode node)
            {/*
                //  Debug.Log("ColorSwitcherLog: Starting PostApply for generatedBody " + generatedBody.name);

                // If the color is stored in "sigmabinaryColor" apply the color to the current orbit
                if (SigmaBinaryLoader.sigmabinaryColor.ContainsKey(generatedBody.name))
                {
                    //  Debug.Log("ColorSwitcherLog: found color for generatedBody " + generatedBody.name);
                    generatedBody.orbitRenderer.orbitColor = SigmaBinaryLoader.sigmabinaryColor[generatedBody.name];
                    generatedBody.orbitRenderer.nodeColor = SigmaBinaryLoader.sigmabinaryColor[generatedBody.name];
                    //  Debug.Log("generatedBody.orbitRenderer.orbitColor = " + generatedBody.orbitRenderer.orbitColor);
                    //  Debug.Log("SigmaBinaryLoader.sigmabinaryColor[generatedBody.name] = " + SigmaBinaryLoader.sigmabinaryColor[generatedBody.name]);
                }
                else
                {
                    //  Debug.Log("ColorSwitcherLog: no colors found for generatedBody " + generatedBody.name);
                    // Otherwise, This might be the color of the Primary Body
                    if (colorSwitcher.ContainsKey(generatedBody.name))
                    {
                        //  Debug.Log("ColorSwitcherLog: the color has been requested for body " + colorSwitcher[generatedBody.name]);
                        // if the color has been requested provide it
                        SigmaBinaryLoader.sigmabinaryColor.Add(colorSwitcher[generatedBody.name], generatedBody.orbitRenderer.orbitColor);
                        //  Debug.Log("ColorSwitcherLog: provided the requested color - sigmabinaryColor.Add(" + colorSwitcher[generatedBody.name] + ", " + generatedBody.orbitRenderer.orbitColor);
                    }
                    else
                    {
                        //  Debug.Log("ColorSwitcherLog: there was no request for the color of generatedBody " + generatedBody.name);
                        // otherwise store it for later
                        SigmaBinaryLoader.sigmabinaryColor.Add(generatedBody.name, generatedBody.orbitRenderer.orbitColor);
                        //  Debug.Log("ColorSwitcherLog: stored for later the color - sigmabinaryColor.Add(" + generatedBody.name + ", " + generatedBody.orbitRenderer.orbitColor);
                    }
                }
                //  Debug.Log("ColorSwitcherLog: Ending PostApply for generatedBody " + generatedBody.name);*/
            }
            public ColorSwitcher ()
            {
            }
        }
    }
}
