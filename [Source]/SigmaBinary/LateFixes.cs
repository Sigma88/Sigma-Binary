﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;
using Kopernicus;
using Kopernicus.Configuration;
using Kopernicus.OnDemand;


namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class LateFixes : MonoBehaviour
    {
        public static void TextureFixer(Body body1, Body body2, List<Body> list)
        {
            if (!SigmaBinary.IamSad)
            {
                if (DateTime.Today.Day == 26 && DateTime.Today.Month == 1)
                {
                    foreach (Body b in list)
                    {
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(b.generatedBody.scaledVersion.GetComponent<Renderer>().material.GetTextureScale("_MainTex").x, -b.generatedBody.scaledVersion.GetComponent<Renderer>().material.GetTextureScale("_MainTex").y));
                        b.generatedBody.scaledVersion.GetComponent<Renderer>().material.SetTextureScale("_BumpMap", new Vector2(b.generatedBody.scaledVersion.GetComponent<Renderer>().material.GetTextureScale("_BumpMap").x, -b.generatedBody.scaledVersion.GetComponent<Renderer>().material.GetTextureScale("_BumpMap").y));
                    }
                }
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
        }

        void Start()
        {
            if (!SigmaBinary.IamSad)
                if (DateTime.Today.Day == 4 && DateTime.Today.Month == 5)
                    GameEvents.onGUIRnDComplexSpawn.Add(InstructorFixer);
        }

        void InstructorFixer()
        {
            if (!SigmaBinary.IamSad)
            {
                System.Random r = new System.Random();
                string[] q =
                new string[]
                {
                    "<b><color=#10B2DA>Obi Wan Kerman:</color></b>\nMay the Force be with you.",
                    "<b><color=#DA1010>Vader Kerman:</color></b>\nI find your lack of faith disturbing.",
                    "<b><color=#DB8310>Akbar Kerman:</color></b>\nIt's a trap!",
                    "<b><color=#10DA53>Yoda Kerman:</color></b>\nDo. Or do not. There is no try.",
                    "<b><color=#946936>Han Kerman:</color></b>\nNever tell me the odds.",
                    "<b><color=#663300>Chewbacca Kerman:</color></b>\nRrraarrwhhgwwr.",
                    "<b><color=#6F2D30>Palpatine Kerman:</color></b>\nThere is a great disturbance in the Force.",
                    "<b><color=#FF80FF>Leia Kerman:</color></b>\nYou are actually goint INTO an asteroid field?",
                    "<b><color=#FFF881>C-3PO Kerman:</color></b>\nDon't get technical with me. What mission? What are you talking about? I've just about had enough of you.",
                    "<b><color=#8080FF>R2-D2 Kerman:</color></b>\nBeep Bloop, Blop Bleep Boop."
                };
                Resources.FindObjectsOfTypeAll<RDArchivesController>().First().instructorText.text = q[r.Next(q.Length)];
            }
        }
    }
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class UpdateFixes : MonoBehaviour
    {
        public string cd = null;
        void Update()
        {
            if (!SigmaBinary.IamSad)
            {
                if (DateTime.Today.Day == 31 && DateTime.Today.Month == 12)
                {
                    if (DateTime.Now.Hour > 11)
                    {
                        cd = "<b><color=#DB8310>Wernher von Kerman:</color></b>\nCountdown for New Year's Day will start in less than ";
                        if (DateTime.Now.Hour < 23)
                            cd = cd + (24 - DateTime.Now.Hour) + " hours";
                        else if (DateTime.Now.Minute < 30)
                            cd = cd + "an hour";
                        else if (DateTime.Now.Minute < 45)
                            cd = cd + "half an hour";
                        else if (DateTime.Now.Minute < 50)
                            cd = cd + "15 minutes";
                        else if (DateTime.Now.Minute < 55)
                            cd = cd + "10 minutes";
                        else if (DateTime.Now.Minute < 59)
                            cd = cd + "5 minutes";

                        cd = cd + ", don't miss it!";

                        if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59 && DateTime.Now.Second < 30)
                            cd = "<b><color=#DB8310>Wernher von Kerman:</color>\nJust " + (60 - DateTime.Now.Second) + " seconds until midnight!</b>";
                        else if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 59)
                            cd = "<b><color=#DB8310>Wernher von Kerman:</color>\n<size=" + (3 * DateTime.Now.Second - 70) + ">" + (60 - DateTime.Now.Second) + "</size></b>";
                    }
                    string text = Resources.FindObjectsOfTypeAll<RDArchivesController>().First().instructorText.text;
                    if (text != cd)
                    {
                        text = cd;
                        Resources.FindObjectsOfTypeAll<RDArchivesController>().First().instructorText.text = text;
                    }
                }
                if (DateTime.Today.Day == 1 && DateTime.Today.Month == 1)
                {
                    if (DateTime.Now.Hour == 0)
                    {
                        cd = "<b><color=#DB8310>Wernher von Kerman:</color></b>\n<size=50>Happy New Year!</size>";
                    }
                    else
                    {
                        cd = "<b><color=#DB8310>Wernher von Kerman:</color></b>\nHappy New Year!";
                    }
                    string text = Resources.FindObjectsOfTypeAll<RDArchivesController>().First().instructorText.text;
                    if (text != cd)
                    {
                        text = cd;
                        Resources.FindObjectsOfTypeAll<RDArchivesController>().First().instructorText.text = text;
                    }
                }
            }
        }
    }
}
