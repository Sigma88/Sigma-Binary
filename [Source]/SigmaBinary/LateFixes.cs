using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Kopernicus.Components;
using Kopernicus.Configuration;
using Kopernicus.OnDemand;
using System.Threading;
using System.Reflection;
using KSP.UI.Screens;



namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class LateFixes : MonoBehaviour
    {
        public static void TextureFixer(Body body1, Body body2, List<Body> list)
        {
            if (DateTime.Today.Day == 14 && DateTime.Today.Month == 2)
            {
                Texture2D MainTex = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == "NewMunSurfaceMapDiffuse").FirstOrDefault() as Texture2D;
                Texture2D BumpMap = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == "NewMunSurfaceMapNormals").FirstOrDefault() as Texture2D;

                foreach (Body b in list)
                {
                    EnumParser<BodyType> type = new EnumParser<BodyType>(b.template == null ? BodyType.Atmospheric : b.template.type);

                    if (type != BodyType.Star)
                    {
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_MainTex", MainTex);
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_BumpMap", BumpMap);

                        if (OnDemandStorage.useOnDemand)
                        {
                            ScaledSpaceDemand demand = b.generatedBody.scaledVersion.GetComponent<ScaledSpaceDemand>();
                            demand.texture = MainTex.name;
                            demand.normals = BumpMap.name;
                        }

                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2(0, 0));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_BumpMap", new Vector2(1, 1));
                    }
                }
            }
            if (SigmaBinary.ListOfBinaries.Count == 0) //DateTime.Today.Day == 14 && DateTime.Today.Month == 3)
            {
                Debug.Log("SigmaBinaryLog: NameFixer");
                string[] P = new string[] { "92653", "58979", "32384", "62643", "38327", "95028", "84197", "16939", "93751", "05820", "97494", "45923", "07816", "40628", "62089", "98628", "03482", "53421", "17067", "98214" };
                Dictionary<CelestialBody, double> pList = new Dictionary<CelestialBody, double>();

                foreach (Body pBody in SigmaBinary.ListOfBodies)
                {
                    Debug.Log("SigmaBinaryLog: ForEach = " + pBody.name);
                    if (pBody.name == "Sun")
                    {
                        Debug.Log("SigmaBinaryLog: SUN");
                        if (!pBody.generatedBody.celestialBody.GetComponent<NameChanger>())
                        {
                            NameChanger changer = pBody.generatedBody.celestialBody.gameObject.AddComponent<NameChanger>();
                            changer.oldName = pBody.name;
                            changer.newName = "3.1415";
                        }
                        else
                            pBody.generatedBody.celestialBody.gameObject.AddComponent<NameChanger>().newName = "3.1415";
                        Debug.Log("SigmaBinaryLog: 1");
                    }
                    else if (Kopernicus.Templates.orbitPatches.ContainsKey(pBody.name) && Kopernicus.Templates.orbitPatches[pBody.name].GetValue("referenceBody") == "Sun")
                    {
                        pList.Add(pBody.generatedBody.celestialBody, pBody.generatedBody.orbitDriver.orbit.semiMajorAxis);
                    }
                    else if (pBody.orbit.referenceBody == "Sun" && !(Kopernicus.Templates.orbitPatches.ContainsKey(pBody.name) && Kopernicus.Templates.orbitPatches[pBody.name].GetValue("referenceBody") != "Sun"))
                    {
                        Debug.Log("SigmaBinaryLog: 2");
                        if (!(pBody.name == "Kerbin" && SigmaBinary.kerbinFixer != "Sun"))
                            pList.Add(pBody.generatedBody.celestialBody, pBody.generatedBody.orbitDriver.orbit.semiMajorAxis);
                        Debug.Log("SigmaBinaryLog: 3");
                    }
                    Debug.Log("SigmaBinaryLog: 4");
                }
                Debug.Log("SigmaBinaryLog: 5");
                int pCount = 0;
                foreach (KeyValuePair<CelestialBody, double> pFix in pList.OrderBy(pKey => pKey.Value))
                {
                    Debug.Log("SigmaBinaryLog: FOREACH2 = " + pFix.Key.name);
                    if (pCount < 20)
                    {
                        Debug.Log("SigmaBinaryLog: 6");
                        if (!pFix.Key.GetComponent<NameChanger>())
                        {
                            Debug.Log("SigmaBinaryLog: 7");
                            NameChanger changer = pFix.Key.gameObject.AddComponent<NameChanger>();
                            changer.oldName = pFix.Key.name;
                            changer.newName = P[pCount];
                        }
                        else
                            pFix.Key.gameObject.AddComponent<NameChanger>().newName = P[pCount];
                        Debug.Log("SigmaBinaryLog: 8");
                        pCount++;
                    }
                    Debug.Log("SigmaBinaryLog: 9");
                }
                Debug.Log("SigmaBinaryLog: 10");
            }
            if (DateTime.Today.Day == 1 && DateTime.Today.Month == 4)
            {
                EnumParser<BodyType> type1 = new EnumParser<BodyType>(body1.template == null ? BodyType.Atmospheric : body1.template.type);
                EnumParser<BodyType> type2 = new EnumParser<BodyType>(body2.template == null ? BodyType.Atmospheric : body2.template.type);

                if (type1.value != BodyType.Star && type2.value != BodyType.Star)
                {
                    Material material1 = new Material(body1.generatedBody.scaledVersion.GetComponent<Renderer>().material);
                    Material material2 = new Material(body2.generatedBody.scaledVersion.GetComponent<Renderer>().material);
                    body1.generatedBody.scaledVersion.GetComponent<Renderer>().material = material2;
                    body2.generatedBody.scaledVersion.GetComponent<Renderer>().material = material1;

                    if (OnDemandStorage.useOnDemand)
                    {
                        ScaledSpaceDemand demand1 = body1.generatedBody.scaledVersion.GetComponent<ScaledSpaceDemand>();
                        ScaledSpaceDemand demand2 = body2.generatedBody.scaledVersion.GetComponent<ScaledSpaceDemand>();
                        demand1.texture = material2.GetTexture("_MainTex").name;
                        demand1.normals = material2.GetTexture("_BumpMap").name;
                        demand2.texture = material1.GetTexture("_MainTex").name;
                        demand2.normals = material1.GetTexture("_BumpMap").name;
                    }
                }
            }
            if (DateTime.Today.Day == 22 && DateTime.Today.Month == 4)
            {
                Texture2D MainTex = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == "KerbinScaledSpace300").FirstOrDefault() as Texture2D;
                Texture2D BumpMap = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == "KerbinScaledSpace401").FirstOrDefault() as Texture2D;

                foreach (Body b in list)
                {

                    EnumParser<BodyType> type = new EnumParser<BodyType>(b.template == null ? BodyType.Atmospheric : b.template.type);

                    if (type != BodyType.Star)
                    {
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_MainTex", MainTex);
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTexture("_BumpMap", BumpMap);

                        if (OnDemandStorage.useOnDemand)
                        {
                            ScaledSpaceDemand demand = b.generatedBody.scaledVersion.GetComponent<ScaledSpaceDemand>();
                            demand.texture = MainTex.name;
                            demand.normals = BumpMap.name;
                        }

                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(1, 1));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2(0, 0));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_BumpMap", new Vector2(1, 1));
                    }
                }
            }
            if (DateTime.Today.Day == 25 && DateTime.Today.Month == 05)
            {
                list.Find(x => x.name == "Sun").generatedBody.celestialBody.bodyDescription = "\n\n\n                        DON'T\n                        PANIC";
                list.Find(x => x.name == "Kerbin").generatedBody.celestialBody.bodyDescription = "Mostly harmless.";
            }
            if (DateTime.Today.Day == 31 && DateTime.Today.Month == 10)
            {
                foreach (Body b in list)
                {
                    if (b.generatedBody.orbitRenderer != null)
                    {
                        b.generatedBody.orbitRenderer.SetColor(new Color(0.5f, 0.25f, 0f, 1f));
                    }
                }
            }
        }

        void Start()
        {
            if (DateTime.Today.Day == 4 && DateTime.Today.Month == 5)
                GameEvents.onGUIRnDComplexSpawn.Add(InstructorFixer);
        }
        void InstructorFixer()
        {
            System.Random r = new System.Random();
            string[] q =
            new string[]
            {
                "<b><color=#10B2DA>Obi Wan Kerman:</color></b>\nMay the Force be with you.",
                "<b><color=#DA1010>Darth Kerman:</color></b>\nYou have failed me for the last time...",
                "<b><color=#DB8310>Admiral Akbar Kerman:</color></b>\nIt's a trap!",
                "<b><color=#10DA53>Yoda Kerman:</color></b>\nDo. Or do not. There is no try.",
                "<b><color=#868686>Han Solo Kerman:</color></b>\nNever tell me the odds.",
                "<b><color=#946936>Chewbacca Kerman:</color></b>\nRrraarrwhhgwwr.",
                "<b><color=#6F2D30>Palpatine Kerman:</color></b>\nThere is a great disturbance in the Force.",
                "<b><color=#FF80FF>Leia Kerman:</color></b>\nYou are actually goint INTO an asteroid field?"
            };
            Resources.FindObjectsOfTypeAll<RDArchivesController>().First().instructorText.text = q[r.Next(q.Length)];
        }
    }
}
