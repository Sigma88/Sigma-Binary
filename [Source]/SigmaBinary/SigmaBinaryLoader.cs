using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;



namespace SigmaBinaryPlugin
{
    [ExternalParserTarget("SigmaBinary")]
    public class SigmaBinaryLoader : ExternalParserTargetLoader, IParserEventSubscriber
    {
        public PeriodFixer periodFixer { get; set; }
        public KerbinFixer kerbinFixer { get; set; }

        [ParserTarget("after", optional = true)]
        public string after
        {
            set { SigmaBinary.sigmabinaryLoadAfter.Add(value, Loader.currentBody); }
        }

        [ParserTarget("name", optional = true)]
        public string sbName
        {
            get
            {
                return sbName;
            }
            set
            {
                SigmaBinary.sigmabinarySBName.Add(Loader.currentBody, value);
                SigmaBinary.sigmabinaryRedrawOrbit.Add(Loader.currentBody);
            }
        }

        [ParserTarget("primaryLocked", optional = true)]
        public NumericParser<bool> primaryLocked
        {
            set { SigmaBinary.sigmabinaryPrimaryLocked.Add(Loader.currentBody, value); }
        }

        [ParserTarget("redrawOrbit", optional = true)]
        public NumericParser<bool> redrawOrbit
        {
            get
            {
                return redrawOrbit;
            }
            set
            {
                if (!value)
                {
                    SigmaBinary.sigmabinaryRedrawOrbit.Remove(Loader.currentBody);
                }
            }
        }

        [ParserTarget("Properties", optional = true, allowMerge = true)]
        public SigmaBinaryPropertiesLoader sigmabinaryproperties { get; set; }

        [ParserTarget("Orbit", optional = true, allowMerge = true)]
        public SigmaBinaryOrbitLoader sigmabinaryorbit { get; set; }

        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            Orbit.FindClosestPoints = SigmaBinary.FindClosestPointsReverted;
            periodFixer = generatedBody.celestialBody.gameObject.AddComponent<PeriodFixer>();
            kerbinFixer = generatedBody.celestialBody.gameObject.AddComponent<KerbinFixer>();
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            /// listOfBinaries = Dictionary ( string sbOrbit.name  or  sbBarycenter.name, sbSecondary )




            if (!SigmaBinary.sigmabinaryLoadAfter.ContainsValue(Loader.currentBody))
                SigmaBinary.ListOfBinaries.Add(sbName + (redrawOrbit.value ? "Orbit":""), Loader.currentBody);

            if (SigmaBinary.sigmabinaryLoadAfter.ContainsKey(generatedBody.name))
            {
                Body body = SigmaBinary.sigmabinaryLoadAfter[generatedBody.name];
                SigmaBinary.ListOfBinaries.Add(SigmaBinary.sigmabinarySBName[body] + (SigmaBinary.sigmabinaryRedrawOrbit.Contains(body) ? "Orbit" : ""), body);
                SigmaBinary.sigmabinaryLoadAfter.Remove(generatedBody.name);
            }
        }
        public SigmaBinaryLoader()
        {
        }
    }

    public class SigmaBinaryPropertiesLoader : BaseLoader, IParserEventSubscriber
    {
        // description for the body
        [ParserTarget("description", optional = true)]
        public string description
        {
            set { SigmaBinary.sigmabinaryDescription.Add(Loader.currentBody, value); }
        }

        void IParserEventSubscriber.Apply(ConfigNode node)
        {
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
        // Orbit Draw Mode
        [ParserTarget("mode", optional = true)]
        public EnumParser<OrbitRenderer.DrawMode> mode
        {
            set { SigmaBinary.sigmabinaryMode.Add(Loader.currentBody, value); }
        }

        // Orbit Icon Mode
        [ParserTarget("icon", optional = true)]
        public EnumParser<OrbitRenderer.DrawIcons> icon
        {
            set { SigmaBinary.sigmabinaryIcon.Add(Loader.currentBody, value); }
        }

        // Orbit Color
        [ParserTarget("color", optional = true)]
        public ColorParser color
        {
            set
            {
                SigmaBinary.sigmabinaryOrbitColor.Add(Loader.currentBody, value);
            }
        }

        // Orbit iconColor
        [ParserTarget("iconColor", optional = true)]
        public ColorParser iconColor
        {
            set
            {
                SigmaBinary.sigmabinaryOrbitColor.Add(Loader.currentBody, value);
            }
        }

        void IParserEventSubscriber.Apply(ConfigNode node)
        {
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
        }

        public SigmaBinaryOrbitLoader()
        {
        }
    }

}
