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

            [ParserTarget("name", optional = true)]
            public string sbName
            {
                get { return sigmabinary.sbName; }
                set { sigmabinary.sbName = value; }
            }

            [ParserTarget("primaryLocked", optional = true)]
            public bool primaryLocked
            {
                get { return sigmabinary.primaryLocked; }
                set { sigmabinary.primaryLocked = value; }
            }

            [ParserTarget("redrawOrbit", optional = true)]
            public bool redrawOrbit
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
                set { Kopernicus.Templates.drawMode.Add(sigmabinary.sbName, value); }
            }

            // Orbit Icon Mode
            [ParserTarget("icon", optional = true)]
            public EnumParser<OrbitRenderer.DrawIcons> icon
            {
                set { Kopernicus.Templates.drawIcons.Add(sigmabinary.sbName, value); }
            }

            // Orbit Color
            [ParserTarget("color", optional = true)]
            public ColorParser color
            {
                get { return sigmabinary.color; }
                set
                {
                    sigmabinary.hasColor = true;
                    sigmabinary.color = value;
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
