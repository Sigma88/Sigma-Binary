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
            {
                // Damn you Thomas! *FACEPALM*
                Debug.Log("ColorSwitcherLog: Starting PostApply for body " + generatedBody.name);

                // If the color is stored in "sigmabinaryColor" apply the color to the current orbit
                if (SigmaBinaryLoader.sigmabinaryColor.ContainsKey(generatedBody.name))
                {
                    Debug.Log("ColorSwitcherLog: found color for body " + generatedBody.name);
                    generatedBody.orbitRenderer.orbitColor = SigmaBinaryLoader.sigmabinaryColor[generatedBody.name];
                    generatedBody.orbitRenderer.nodeColor = SigmaBinaryLoader.sigmabinaryColor[generatedBody.name];
                    Debug.Log("ColorSwitcherLog: applied color " + SigmaBinaryLoader.sigmabinaryColor[generatedBody.name].ToString());
                }
                else
                {
                    Debug.Log("ColorSwitcherLog: no colors found for body " + generatedBody.name);
                    // Otherwise, This might be the color of the Primary Body
                    if (colorSwitcher.ContainsKey(generatedBody.name))
                    {
                        Debug.Log("ColorSwitcherLog: the color has been requested for body " + colorSwitcher[generatedBody.name]);
                        // if the color has been requested provide it
                        SigmaBinaryLoader.sigmabinaryColor.Add(colorSwitcher[generatedBody.name], generatedBody.orbitRenderer.orbitColor);
                        Debug.Log("ColorSwitcherLog: provided the requested color " + generatedBody.orbitRenderer.orbitColor);
                    }
                    else
                    {
                        Debug.Log("ColorSwitcherLog: there was no request for the color of body " + generatedBody.name);
                        // otherwise store it for later
                        SigmaBinaryLoader.sigmabinaryColor.Add(generatedBody.name, generatedBody.orbitRenderer.orbitColor);
                        Debug.Log("ColorSwitcherLog: stored for later the color " + generatedBody.orbitRenderer.orbitColor);
                    }
                }
                Debug.Log("ColorSwitcherLog: Ending PostApply for body " + generatedBody.name);
            }
            public ColorSwitcher ()
            {
            }
        }
    }
}
