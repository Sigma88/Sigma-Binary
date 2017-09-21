using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;


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


                PSystemBody newParent = ParentOf(barycenter);

                if (newParent != null && newParent.children.Contains(barycenter))
                {
                    newParent.children.Insert(newParent.children.IndexOf(barycenter), primary);
                    Debug.Log("ArchivesFixer", "Inserted primary " + primary.name + " in the children list of parent " + newParent.name + " at index = " + newParent?.children?.IndexOf(primary));

                    newParent.children.Remove(barycenter);
                    Debug.Log("ArchivesFixer", "Removed barycenter " + barycenter.name + " from the children list of parent " + newParent.name);
                }
                else
                {
                    Debug.Log("ArchivesFixer", "Failed to remove body " + primary.name + " from the children list of parent " + (string.IsNullOrEmpty(newParent?.name) ? "(null)" : newParent.name) + ".");
                    continue;
                }
            }

            FieldInfo list = typeof(RDArchivesController).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Skip(7).First();
            Debug.Log("ArchivesFixer", "FieldInfo 'list' should be 'searchFilter_planets', FieldInfo name is: " + list.Name);

            MethodInfo add = typeof(RDArchivesController).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Skip(26).First();
            Debug.Log("ArchivesFixer", "MethodInfo 'add' should be 'AddPlanets', MethodInfo name is: " + add.Name);


            var archivesFixer = Resources.FindObjectsOfTypeAll<RDArchivesController>().First();


            list.SetValue(archivesFixer, new Dictionary<string, List<RDArchivesController.Filter>>());
            Debug.Log("ArchivesFixer", "Cleared list 'searchFilter_planets' as the 'AddPlanets' method requires it to be empty.");

            add.Invoke(archivesFixer, null);
            Debug.Log("ArchivesFixer", "Invoked method 'AddPlanets' to make sure the science archives show the planets in the correct order.");
        }

        PSystemBody ParentOf(PSystemBody child)
        {
            return PSystemManager.Instance?.systemPrefab?.GetComponentsInChildren<PSystemBody>(true)?.FirstOrDefault(b => b.children.Contains(child));
        }
    }
}
