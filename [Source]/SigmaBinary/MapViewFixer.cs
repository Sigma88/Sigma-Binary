using System.Linq;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens.Mapview;
using KSP.UI.Screens.Mapview.MapContextMenuOptions;
using Kopernicus.Components;


namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class MapViewFixer : MonoBehaviour
    {
        FieldInfo[] fields;

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
                    Debug.Log("MapViewFixer", "Name of FieldInfo 'mode_f' should be 'MenuDrawMode'. The name is = " + mode_f?.Name);

                    FieldInfo context_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(MapContextMenu));
                    Debug.Log("MapViewFixer", "Name of FieldInfo 'context_f' should be 'ContextMenu'. The name is = " + context_f?.Name);

                    FieldInfo cast_f = typeof(OrbitTargeter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.FieldType == typeof(OrbitRenderer.OrbitCastHit));
                    Debug.Log("MapViewFixer", "Name of FieldInfo 'cast_f' should be 'orbitCastHit'. The name is = " + cast_f?.Name);

                    fields = new FieldInfo[] { mode_f, context_f, cast_f };
                }

                if (FlightGlobals.ActiveVessel != null)
                {
                    OrbitTargeter targeter = FlightGlobals.ActiveVessel.orbitTargeter;

                    int mode = (int)fields[0].GetValue(targeter);
                    if (mode == 2)
                    {
                        OrbitRenderer.OrbitCastHit cast = (OrbitRenderer.OrbitCastHit)fields[2].GetValue(targeter);

                        CelestialBody body = PSystemManager.Instance.localBodies.Find(b => b.name == cast.or.discoveryInfo.name.Value);

                        if (SigmaBinary.mapViewFixerList.ContainsKey(body))
                        {
                            CelestialBody body2 = SigmaBinary.mapViewFixerList[body];

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
