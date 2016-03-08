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
    }
}

