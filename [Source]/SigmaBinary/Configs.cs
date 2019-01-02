using System.Linq;
using KSP.Localization;


namespace SigmaBinaryPlugin
{
    public class Configs
    {
        static UrlDir.UrlConfig oldKopernicus;
        static ConfigNode newKopernicus;
        static ConfigNode[] oldBodies;

        public static void ModuleManagerPostLoad()
        {
            oldKopernicus = GameDatabase.Instance?.GetConfigs("Kopernicus")?.FirstOrDefault();
            if (oldKopernicus == null) return;
            LoadBodies();
        }

        static void LoadBodies()
        {
            newKopernicus = oldKopernicus.config;

            oldBodies = newKopernicus.GetNodes("Body");
            newKopernicus.RemoveNodes("Body");

            CheckBodies();
        }

        static void CheckBodies()
        {
            for (int i = 0; i < oldBodies.Length; i++)
            {
                ConfigNode SigmaBinary = oldBodies[i].GetNode("SigmaBinary");

                if (SigmaBinary != null)
                {
                    ConfigNode Orbit = oldBodies[i].GetNode("Orbit");
                    string referenceBody = Orbit.GetValue("referenceBody");

                    if (!string.IsNullOrEmpty(SigmaBinary?.GetValue("name")) && !string.IsNullOrEmpty("referenceBody") && referenceBody != "Sun")
                    {
                        if (!SigmaBinary.HasValue("name"))
                            SigmaBinary.AddValue("name", referenceBody + oldBodies[i].GetValue("name"));

                        ConfigNode Properties = new ConfigNode("Properties");

                        if (SigmaBinary.HasNode("Properties"))
                            Properties = SigmaBinary.GetNode("Properties");

                        if (!Properties.HasValue("description"))
                        {
                            Properties.AddValue("description", Localizer.Format("#SB-LOC_001"));
                            SigmaBinary.RemoveNodes("Properties");
                            SigmaBinary.AddNode(Properties);
                        }

                        if (!bool.TryParse(SigmaBinary.GetValue("redrawOrbit"), out bool redrawOrbit))
                            redrawOrbit = true;

                        if (redrawOrbit)
                            GenerateOrbitReplacement(SigmaBinary.GetValue("name"), Orbit.CreateCopy());

                        GenerateBarycenters(SigmaBinary.GetValue("name"));
                    }
                    else
                    {
                        oldBodies[i].RemoveNodes("SigmaBinary");
                    }
                }

                newKopernicus.AddNode(oldBodies[i]);
            }

            oldKopernicus.config = newKopernicus;
        }

        static void GenerateOrbitReplacement(string name, ConfigNode Orbit)
        {
            ConfigNode SigmaOrbit = new ConfigNode("Body");

            SigmaOrbit.AddValue("name", name + "Orbit");

            Orbit.RemoveValues("referenceBody");
            Orbit.AddValue("referenceBody", name);
            Orbit.AddValue("icon", "0");
            Orbit.AddValue("mode", "3");
            SigmaOrbit.AddNode(Orbit);

            string data = "barycenter = True\ncontractWeight = 0\nTemplate\n{\nname = Jool\n}\nProperties\n{\nradius = 61\nRnDVisibility = HIDDEN\n}\nAtmosphere\n{\nenabled = false\n}\nScaledVersion\n{\ntype = Vacuum\nfadeStart = 0\nfadeEnd = 0\nMaterial\n{\ncolor = 0,0,0,0\nshininess = 0\n}\n}\nDebug\n{\nexportMesh = false\n}";
            SigmaOrbit.AddData(ConfigNode.Parse(data));

            newKopernicus.AddNode(SigmaOrbit);
        }

        static void GenerateBarycenters(string name)
        {
            ConfigNode SigmaBarycenter = new ConfigNode("Body");
            SigmaBarycenter.AddValue("name", name);
            SigmaBarycenter.AddData(ConfigNode.Parse("contractWeight = 0\nTemplate\n{\nname = Jool\n}\nProperties\n{\nradius = 61\nRnDVisibility = SKIP\n}\nOrbit\n{\nreferenceBody = Sun\n}\nAtmosphere\n{\nenabled = false\n}\nScaledVersion\n{\ntype = Vacuum\nfadeStart = 0\nfadeEnd = 0\nMaterial\n{\ncolor = 0,0,0,0\nshininess = 0\n}\n}\nDebug\n{\nexportMesh = false\n}"));
            newKopernicus.AddNode(SigmaBarycenter);
        }
    }
}
