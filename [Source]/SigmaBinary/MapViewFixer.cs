 
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
    public class MapViewFixer : MonoBehaviour
    {
        private FieldInfo[] fields;
        
        void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
        void LateUpdate()
        {
            if (MapView.MapIsEnabled)
            {
                if (fields == null)
                {
                    FieldInfo mode_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType.Name.EndsWith("MenuDrawMode"));
                    FieldInfo context_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(MapContextMenu));
                    FieldInfo cast_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(OrbitRenderer.OrbitCastHit));
                    fields = new FieldInfo[] { mode_f, context_f, cast_f };
                }
                if (FlightGlobals.ActiveVessel != null)
                {
                    OrbitTargeter targeter = FlightGlobals.ActiveVessel.orbitTargeter;
                    
                    Int32 mode = (Int32)fields[0].GetValue(targeter);
                    if (mode == 2)
                    {
                        OrbitRenderer.OrbitCastHit cast = (OrbitRenderer.OrbitCastHit)fields[2].GetValue(targeter);

                        CelestialBody body = PSystemManager.Instance.localBodies.Find(b => b.name == cast.or.discoveryInfo.name.Value);

                        if (SigmaBinary.mapViewFixerList.ContainsKey(body.transform.name))
                        {
                            CelestialBody body2 = PSystemManager.Instance.localBodies.Find(b => b.transform.name == SigmaBinary.mapViewFixerList[body.transform.name]);

                            ((MapContextMenu)fields[1].GetValue(targeter)).Dismiss();
                            
                            MapContextMenu context =
                                MapContextMenu.Create
                                (
                                    body2.GetComponent<NameChanger>() ? body2.GetComponent<NameChanger>().newName : body2.name,
                                    new Rect(0.5f, 0.5f, 300f, 75f), cast, () =>
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
                    }
                }
            }
        }
    }
}