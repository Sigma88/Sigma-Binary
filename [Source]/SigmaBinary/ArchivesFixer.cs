using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using KSP.UI.Screens;
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
            FieldInfo s1 = typeof(RDPlanetListItemContainer).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Skip(1).First();
            FieldInfo s2 = typeof(RDPlanetListItemContainer).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Skip(2).First();
            FieldInfo s3 = typeof(RDPlanetListItemContainer).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Skip(3).First();

            List<RDPlanetListItemContainer> itemsList = new List<RDPlanetListItemContainer>();
            itemsList.AddRange(Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>());

            foreach (RDPlanetListItemContainer item in itemsList)
            {
                Debug.Log("SigmaBinaryLog: item.name = " + item.name);
                Debug.Log("SigmaBinaryLog: item.label = " + item.label_planetName.text);
                if (SigmaBinary.archivesFixerList.ContainsKey(item.label_planetName.text))
                {
                    if (item.hierarchy_level == 2)
                    {
                        if (SigmaBinary.archivesFixerList[item.label_planetName.text] == 0)
                            SigmaBinary.archivesFixerList[item.label_planetName.text] = 1.6f * (float)s2.GetValue(item);

                        item.planet.transform.localScale = new Vector3(SigmaBinary.archivesFixerList[item.label_planetName.text], SigmaBinary.archivesFixerList[item.label_planetName.text], SigmaBinary.archivesFixerList[item.label_planetName.text]);

                        s2.SetValue(item, SigmaBinary.archivesFixerList[item.label_planetName.text]);
                        s3.SetValue(item, 1.1f * SigmaBinary.archivesFixerList[item.label_planetName.text]);

                        item.parent.gameObject.SetActive(false);
                        itemsList.Find(c => c.label_planetName.text == "Sun").AddChild(item);

                        item.layoutElement.preferredHeight = 80f;
                        item.hierarchy_level = 1;
                        item.Show(1);
                    }
                    else if (item.hierarchy_level > 2)
                    {
                        item.parent.gameObject.SetActive(false);
                        List<RDPlanetListItemContainer> children = (List<RDPlanetListItemContainer>)s1.GetValue(item.parent.parent);

                        children.Remove(item.parent);
                        s1.SetValue(item.parent.parent, children);

                        item.parent.parent.AddChild(item);
                        item.hierarchy_level = item.hierarchy_level - 1;
                    }
                }
            }
        }
    }
}
