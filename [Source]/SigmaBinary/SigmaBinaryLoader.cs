using Kopernicus.ConfigParser;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration;
using Kopernicus.Configuration.Parsing;


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
                SigmaBinary.sigmabinarySBName.Add(Parser.GetState<Body>("Kopernicus:currentBody"), value);
                SigmaBinary.sigmabinaryRedrawOrbit.Add(Parser.GetState<Body>("Kopernicus:currentBody"));
            }
        }

        [ParserTarget("after", Optional = true)]
        public string after
        {
            set
            {
                if (!SigmaBinary.ListOfBinaries.ContainsValue(SigmaBinary.ListOfBodies.Find(b => b.GeneratedBody.name == value)))
                    SigmaBinary.sigmabinaryLoadAfter.Add(value, Parser.GetState<Body>("Kopernicus:currentBody"));
            }
        }

        [ParserTarget("primaryLocked", Optional = true)]
        public NumericParser<bool> primaryLocked
        {
            set
            {
                if (value)
                    SigmaBinary.sigmabinaryPrimaryLocked.Add(Parser.GetState<Body>("Kopernicus:currentBody"));
            }
        }

        [ParserTarget("redrawOrbit", Optional = true)]
        public NumericParser<bool> redrawOrbit
        {
            set
            {
                if (!value)
                    SigmaBinary.sigmabinaryRedrawOrbit.Remove(Parser.GetState<Body>("Kopernicus:currentBody"));
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
            if (!SigmaBinary.sigmabinaryLoadAfter.ContainsValue(Parser.GetState<Body>("Kopernicus:currentBody")))
            {
                SigmaBinary.ListOfBinaries.Add(SigmaBinary.sigmabinarySBName[Parser.GetState<Body>("Kopernicus:currentBody")], Parser.GetState<Body>("Kopernicus:currentBody"));
                LoadAfter(Parser.GetState<Body>("Kopernicus:currentBody"));
            }
        }

        public void LoadAfter(Body currentBody)
        {
            if (SigmaBinary.sigmabinaryLoadAfter.ContainsKey(currentBody.GeneratedBody.name))
            {
                Body body = SigmaBinary.sigmabinaryLoadAfter[currentBody.GeneratedBody.name];
                SigmaBinary.ListOfBinaries.Add(SigmaBinary.sigmabinarySBName[body], body);
                SigmaBinary.sigmabinaryLoadAfter.Remove(currentBody.GeneratedBody.name);
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
                SigmaBinary.sigmabinaryDescription.Add(Parser.GetState<Body>("Kopernicus:currentBody"), value);
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
                SigmaBinary.sigmabinaryMode.Add(Parser.GetState<Body>("Kopernicus:currentBody"), value);
            }
        }

        // Orbit Icon Mode
        [ParserTarget("icon", Optional = true)]
        public EnumParser<OrbitRenderer.DrawIcons> icon
        {
            set
            {
                SigmaBinary.sigmabinaryIcon.Add(Parser.GetState<Body>("Kopernicus:currentBody"), value);
            }
        }

        // Orbit Color
        [ParserTarget("color", Optional = true)]
        public ColorParser color
        {
            set
            {
                SigmaBinary.sigmabinaryOrbitColor.Add(Parser.GetState<Body>("Kopernicus:currentBody"), value.Value);
            }
        }

        // Orbit iconColor
        [ParserTarget("iconColor", Optional = true)]
        public ColorParser iconColor
        {
            set
            {
                SigmaBinary.sigmabinaryIconColor.Add(Parser.GetState<Body>("Kopernicus:currentBody"), value.Value);
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
