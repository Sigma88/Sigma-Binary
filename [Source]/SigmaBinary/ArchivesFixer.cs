using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;
using Kopernicus.RuntimeUtility;


namespace SigmaBinaryPlugin
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class ArchivesFixerComponent : MonoBehaviour
    {
        void Update()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && SigmaBinary.archivesFixerList?.Count > 0)
            {
                RDArchivesController[] items = Resources.FindObjectsOfTypeAll<RDArchivesController>();

                for (int i = 0; i < items?.Length; i++)
                {
                    RDArchivesController item = items[i];
                    item.gameObject.AddOrGetComponent<ArchivesFixer>();
                }
            }
        }
    }

    class ArchivesFixer : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("ArchivesFixer", "Start");
            for (int i = 0; i < SigmaBinary.archivesFixerList?.Count; i++)
            {
                KeyValuePair<PSystemBody, PSystemBody> pair = SigmaBinary.archivesFixerList.ElementAt(i);
                PSystemBody primary = pair.Key;
                PSystemBody barycenter = pair.Value;
                PSystemBody oldParent = ParentOf(primary);

                if (oldParent != null && oldParent.children.Contains(primary))
                {
                    oldParent.children.Remove(primary);
                    Debug.Log("ArchivesFixer", "Removed primary " + primary.name + " from the children list of oldParent " + oldParent.name);
                }
                else
                {
                    Debug.Log("ArchivesFixer", "Failed to remove primary " + primary.name + " from the children list of oldParent " + (string.IsNullOrEmpty(oldParent?.name) ? "(null)" : oldParent.name) + ".");
                    continue;
                }

                //PSystemBody newParent = ParentOf(barycenter);
                barycenter.children.Add(primary);
            }

            MethodInfo AddPlanets = typeof(RnDFixer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)?.Skip(2)?.FirstOrDefault();
            Debug.Log("ArchivesFixer", "MethodInfo.Name is = '" + AddPlanets?.Name + "()', should be = 'AddPlanets()'");
            AddPlanets.Invoke(Resources.FindObjectsOfTypeAll<RnDFixer>().FirstOrDefault().gameObject, null);
            Debug.Log("ArchivesFixer", "End");
        }

        PSystemBody ParentOf(PSystemBody child)
        {
            return PSystemManager.Instance?.systemPrefab?.GetComponentsInChildren<PSystemBody>(true)?.FirstOrDefault(b => b.children.Contains(child));
        }
    }
}
