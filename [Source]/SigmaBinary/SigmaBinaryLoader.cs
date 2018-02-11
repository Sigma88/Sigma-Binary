using Kopernicus;
using Kopernicus.Configuration;


namespace SigmaBinaryPlugin
{
    [ParserTargetExternal("Body", "SigmaBinary", "Kopernicus")]
    public class SigmaBinaryLoader : BaseLoader, IParserEventSubscriber
    {
        [ParserTarget("debug", Optional = true)]
        public NumericParser<bool> debug
        {
            set
            {
                if (!Debug.debug)
                    Debug.debug = value?.Value == true;
            }
        }

        [ParserTarget("name", Optional = true)]
        public string sbName
        {
            set
            {
                SigmaBinary.sigmabinarySBName.Add(Loader.currentBody, value);
                SigmaBinary.sigmabinaryRedrawOrbit.Add(Loader.currentBody);
            }
        }

        [ParserTarget("after", Optional = true)]
        public string after
        {
            set
            {
                if (!SigmaBinary.ListOfBinaries.ContainsValue(SigmaBinary.ListOfBodies.Find(b => b.name == value)))
                    SigmaBinary.sigmabinaryLoadAfter.Add(value, Loader.currentBody);
            }
        }

        [ParserTarget("primaryLocked", Optional = true)]
        public NumericParser<bool> primaryLocked
        {
            set
            {
                if (value)
                    SigmaBinary.sigmabinaryPrimaryLocked.Add(Loader.currentBody);
            }
        }

        [ParserTarget("redrawOrbit", Optional = true)]
        public NumericParser<bool> redrawOrbit
        {
            set
            {
                if (!value)
                    SigmaBinary.sigmabinaryRedrawOrbit.Remove(Loader.currentBody);
            }
        }

        [ParserTarget("Properties", Optional = true, AllowMerge = true)]
        public SigmaBinaryPropertiesLoader sigmabinaryproperties { get; set; }

        [ParserTarget("Orbit", Optional = true, AllowMerge = true)]
        public SigmaBinaryOrbitLoader sigmabinaryorbit { get; set; }

        void IParserEventSubscriber.Apply(ConfigNode node)
        {
            Orbit.FindClosestPoints = new Orbit.FindClosestPointsDelegate(EncounterMathFixer.FindClosestPointsRevertedCauseNewOneSucks);
            Orbit.SolveClosestApproach = new Orbit.SolveClosestApproachDelegate(EncounterMathFixer.SolveClosestApproachWithoutComplaining);
            PatchedConics.CheckEncounter = new PatchedConics.CheckEncounterDelegate(EncounterMathFixer.CheckEncounterButDontBitchAboutIt);
        }

        void IParserEventSubscriber.PostApply(ConfigNode node)
        {
            if (!SigmaBinary.sigmabinaryLoadAfter.ContainsValue(Loader.currentBody))
            {
                SigmaBinary.ListOfBinaries.Add(SigmaBinary.sigmabinarySBName[Loader.currentBody], Loader.currentBody);
                LoadAfter(Loader.currentBody);
            }
        }

        public void LoadAfter(Body currentBody)
        {
            if (SigmaBinary.sigmabinaryLoadAfter.ContainsKey(currentBody.name))
            {
                Body body = SigmaBinary.sigmabinaryLoadAfter[currentBody.name];
                SigmaBinary.ListOfBinaries.Add(SigmaBinary.sigmabinarySBName[body], body);
                SigmaBinary.sigmabinaryLoadAfter.Remove(currentBody.name);
                LoadAfter(body);
            }
        }

        public SigmaBinaryLoader()
        {
        }
    }

    public class SigmaBinaryPropertiesLoader : BaseLoader, IParserEventSubscriber
    {
        // description for the body
        [ParserTarget("description", Optional = true)]
        public string description
        {
            set
            {
                SigmaBinary.sigmabinaryDescription.Add(Loader.currentBody, value);
            }
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
        [ParserTarget("mode", Optional = true)]
        public EnumParser<OrbitRenderer.DrawMode> mode
        {
            set
            {
                SigmaBinary.sigmabinaryMode.Add(Loader.currentBody, value);
            }
        }

        // Orbit Icon Mode
        [ParserTarget("icon", Optional = true)]
        public EnumParser<OrbitRenderer.DrawIcons> icon
        {
            set
            {
                SigmaBinary.sigmabinaryIcon.Add(Loader.currentBody, value);
            }
        }

        // Orbit Color
        [ParserTarget("color", Optional = true)]
        public ColorParser color
        {
            set
            {
                SigmaBinary.sigmabinaryOrbitColor.Add(Loader.currentBody, value.Value);
            }
        }

        // Orbit iconColor
        [ParserTarget("iconColor", Optional = true)]
        public ColorParser iconColor
        {
            set
            {
                SigmaBinary.sigmabinaryIconColor.Add(Loader.currentBody, value.Value);
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
