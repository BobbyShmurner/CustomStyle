using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using System.Reflection;
using System.Collections.Generic;

namespace CustomStyle {
	[HarmonyPatch(typeof(StyleHUD))]
	[HarmonyPatch("Start")]
	public static class FixStyleInfoText {
		public static void Postfix(StyleHUD __instance, ref Text ___styleInfo) {
			___styleInfo = __instance.transform.GetChild(0).GetChild(1).GetComponentInChildren<Text>();
		}
	}

	[HarmonyPatch]
	public static class UpdateStyleRank {
		public static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.Method(typeof(StyleHUD), "AscendRank");
			yield return AccessTools.Method(typeof(StyleHUD), "DescendRank");
		}

		public static void Postfix(StyleHUD __instance) {
			Plugin.CustomStyle.SetStyle(Plugin.Styles[__instance.currentRank]);
		}
	}

	[HarmonyPatch(typeof(StyleHUD))]
    [HarmonyPatch("Update")]
	public static class InfinteStyle {
		public static void Postfix(ref StyleHUD __instance, ref bool ___ascending) {
			// __instance.currentMeter = __instance.maxMeter;
			__instance.styleRank.enabled = Plugin.showOriginalStyle;

			// -- Ascention / Descention Code --

			___ascending = Plugin.shouldAscend || Plugin.shouldDescend;

			if (Plugin.shouldAscend && __instance.currentRank < 7) typeof(StyleHUD).GetMethod("AscendRank", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new Object[]{});
			if (Plugin.shouldDescend) typeof(StyleHUD).GetMethod("DescendRank", BindingFlags.Instance | BindingFlags.Public).Invoke(__instance, new Object[]{});

			Plugin.shouldAscend = false;
			Plugin.shouldDescend = false;
		}
	}
}
