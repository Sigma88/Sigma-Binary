 
using UnityEngine;
using Kopernicus.Components;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using JetBrains.Annotations;
using Kopernicus.Configuration;
using KSP.UI.Screens;
using KSP.UI.Screens.Mapview;
using KSP.UI.Screens.Mapview.MapContextMenuOptions;

namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RuntimeUtility : MonoBehaviour
    {
        private FieldInfo[] fields;
        
        void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
        void LateUpdate()
        {
            Debug.Log("SigmaBinaryLog: 1");
            if (MapView.MapIsEnabled)
            {
                Debug.Log("SigmaBinaryLog: 2");
                if (fields == null)
                {
                    Debug.Log("SigmaBinaryLog: 3");
                    FieldInfo mode_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType.Name.EndsWith("MenuDrawMode"));
                    FieldInfo context_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(MapContextMenu));
                    FieldInfo cast_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(OrbitRenderer.OrbitCastHit));
                    fields = new FieldInfo[] { mode_f, context_f, cast_f };
                    Debug.Log("SigmaBinaryLog: 4");
                }
                Debug.Log("SigmaBinaryLog: 5");
                OrbitTargeter targeter = FlightGlobals.ActiveVessel.orbitTargeter;
                Int32 mode = (Int32) fields[0].GetValue(targeter);
                Debug.Log("SigmaBinaryLog: 6");
                if (mode == 2)
                {
                    Debug.Log("SigmaBinaryLog: 7");
                    OrbitRenderer.OrbitCastHit cast = (OrbitRenderer.OrbitCastHit) fields[2].GetValue(targeter);
                    Debug.Log("SigmaBinaryLog: cast.or.discoveryInfo.name.Value = " + cast.or.discoveryInfo.name.Value);
                    CelestialBody body = PSystemManager.Instance.localBodies.Find(b => b.name == cast.or.discoveryInfo.name.Value);
                    Debug.Log("SigmaBinaryLog: body = " + body);
                    if (SigmaBinary.mapViewFixerList.ContainsKey(body.transform.name))
                    {
                        Debug.Log("SigmaBinaryLog: 8");
                        Debug.Log("SigmaBinaryLog: body2 = " + body2);
                        CelestialBody body2 = PSystemManager.Instance.localBodies.Find(b => b.name == SigmaBinary.mapViewFixerList[body.transform.name].transform.name);

                        ((MapContextMenu)fields[1].GetValue(targeter)).Dismiss();

                        Debug.Log("SigmaBinaryLog: 9");
                        MapContextMenu context = 
                            MapContextMenu.Create
                            (
                                body2.name,
                                new Rect(0.5f, 0.5f, 300f, 50f), cast, () =>
                                {
                                    fields[0].SetValue(targeter, 0);
                                    fields[1].SetValue(targeter, null);
                                },
                                new SetAsTarget
                                (
                                    body2.orbitDriver.Targetable,
                                    () => FlightGlobals.fetch.VesselTarget
                                ),
                                new FocusObject
                                (
                                    body2.orbitDriver
                                )
                            );
                        fields[1].SetValue(targeter, context);
                    }

                    Debug.Log("SigmaBinaryLog: 10");
                }
                Debug.Log("SigmaBinaryLog: 11");
            }
            Debug.Log("SigmaBinaryLog: 12");
        }
    }
}
