using System.Collections.Generic;
using UnityEngine;
using SigmaBinaryPlugin.Components;
using SigmaBinaryPlugin.Configuration;
using System;
using System.Reflection;
using System.Linq;

namespace SigmaBinaryPlugin
{
	namespace Configuration
	{
		[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
		public class ArchivesFixer : MonoBehaviour
		{
			void Update()
			{
				foreach (RDPlanetListItemContainer planetItem in Resources.FindObjectsOfTypeAll<RDPlanetListItemContainer>())
				{
					if (SigmaBinaryLoader.ArchivesFixerList.Contains(planetItem.label_planetName.text))
					{
                        planetItem.gameObject.SetActive(false);
                    }
				}
			}
		}
	}
}
