using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;


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
            Debug.Log("SigmaLog: SigmaBinary ArchivesFixer");
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
            
            Kopernicus.RnDFixer.AddPlanets();
        }

        PSystemBody ParentOf(PSystemBody child)
        {
            return PSystemManager.Instance?.systemPrefab?.GetComponentsInChildren<PSystemBody>(true)?.FirstOrDefault(b => b.children.Contains(child));
        }
    }
}
