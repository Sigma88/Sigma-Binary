using System.Collections.Generic;
using UnityEngine;
using SigmaBinaryPlugin.Configuration;
using System;
using System.Reflection;
using System.Linq;
using KSP.UI.Screens;
using Kopernicus.Components;

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
					if (SigmaBinary.archivesFixerList.Contains(planetItem.label_planetName.text))
					{
						planetItem.gameObject.SetActive(false);
					}
				}
			}
		}
	}
}
