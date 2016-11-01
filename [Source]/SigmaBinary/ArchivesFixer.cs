using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Reflection;
using System.Linq;
using KSP.UI.Screens;
using KSP.UI;
using Kopernicus.Components;

namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ArchivesFixerComponent : MonoBehaviour
    {
        void Update()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                foreach (RDArchivesController item in Resources.FindObjectsOfTypeAll<RDArchivesController>())
                    item.gameObject.AddOrGetComponent<ArchivesFixer>();
            }
        }
    }
    
    public class ArchivesFixer : MonoBehaviour
    {
        void Start()
        {
            PSystemBody sun = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == "Sun");
            foreach (string P in SigmaBinary.archivesFixerList.Keys)
            {
                PSystemBody primary = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == P);
                PSystemBody barycenter = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == SigmaBinary.archivesFixerList[P][0]);
                PSystemBody reference = PSystemManager.Instance.systemPrefab.GetComponentsInChildren<PSystemBody>(true).First(b => b.name == SigmaBinary.archivesFixerList[P][1]);


                if (primary.name == "Kerbin")
                    sun.children.Remove(primary);

                int index = reference.children.IndexOf(barycenter);

                reference.children.Remove(barycenter);
                reference.children.Insert(index, primary);
            }

            FieldInfo list = typeof(RDArchivesController).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Skip(7).First();
            MethodInfo add = typeof(RDArchivesController).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Skip(27).First();

            var archivesFixer = Resources.FindObjectsOfTypeAll<RDArchivesController>().First();

            list.SetValue(archivesFixer, new Dictionary<string, List<RDArchivesController.Filter>>());
            add.Invoke(archivesFixer, null);
        }
    }
}
