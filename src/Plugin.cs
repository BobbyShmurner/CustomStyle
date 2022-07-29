using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

using ULTRAINTERFACE;

using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace CustomStyle
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony harmony;

        internal static bool shouldAscend = false;
        internal static bool shouldDescend = false;
        internal static bool showOriginalStyle = false;

        internal static List<StyleData> Styles = new List<StyleData>();
        internal static CustomStyleText CustomStyle;
        
        private void Awake()
        {
            Logger.LogInfo($"Loading Plugin {PluginInfo.PLUGIN_GUID}...");

            Plugin.Log = base.Logger;
            Plugin.Log.LogInfo("Created Global Logger");

            harmony = new Harmony("CustomStyle");
            harmony.PatchAll();
            Plugin.Log.LogInfo("Applied All Patches");

            var assembly = typeof(Plugin).GetTypeInfo().Assembly;
            Stream resourceStream = assembly.GetManifestResourceStream("CustomStyle.resources.customstyle");

            var bundle = AssetBundle.LoadFromStream(resourceStream);
            CustomStyleText.StyleFont = bundle.LoadAsset<Font>("BebasNeue-Regular.ttf");
            bundle.Unload(false);

            Plugin.Log.LogInfo($"Loaded Font");

            // Styles.Add(new StyleData("D", "estructive", new Color(0, 0.58f, 1, 1), Color.white));
            // Styles.Add(new StyleData("C", "haotic", new Color(0.29f, 1, 0, 1), Color.white));
            // Styles.Add(new StyleData("B", "rutal", new Color(1, 0.85f, 0, 1), Color.white));
            // Styles.Add(new StyleData("A", "narchic", new Color(1, 0.42f, 0, 1), Color.white));
            // Styles.Add(new StyleData("S", "upreme", Color.red, Color.white));
            // Styles.Add(new StyleData("SS", "adistic", Color.red, Color.white));
            // Styles.Add(new StyleData("SSS", "hitstorm", Color.red, Color.white));
            // Styles.Add(new StyleData("ULTRAKILL", "", new Color(1, 0.85f, 0, 1), new Color(1, 0.42f, 0, 1), true));

            Styles.Add(new StyleData("D", "eez nuts", new Color(0, 0.58f, 1, 1), Color.white));
            Styles.Add(new StyleData("C", "um.", new Color(0.29f, 1, 0, 1), Color.white));
            Styles.Add(new StyleData("B", "ozo", new Color(1, 0.85f, 0, 1), Color.white));
            Styles.Add(new StyleData("A", "ss", new Color(1, 0.42f, 0, 1), Color.white));
            Styles.Add(new StyleData("S", "ad :(", Color.red, Color.white));
            Styles.Add(new StyleData("SS", "hhhhh", Color.red, Color.white));
            Styles.Add(new StyleData("SSS", "uck ur mum", Color.red, Color.white));
            Styles.Add(new StyleData("ULTRA ASS", "", new Color(1, 0.85f, 0, 1), new Color(1, 0.42f, 0, 1), true));

            Plugin.Log.LogInfo("Created Style Data");

            UI.RegisterOnSceneLoad((scene) => {
                CustomStyle = CustomStyleText.Create(Camera.main.GetComponentInChildren<StyleHUD>().styleRank.transform, Styles[0]);
            });

            Options.CreateOptionsMenu("Custom Styles", (menu) => {
                var panel = menu.AddOptionsPanel();
                UI.CreateButton(panel, "epic button 1");
                UI.CreateButton(panel, "epic button 2");
                UI.CreateButton(panel, "epic button 3");
                UI.CreateButton(panel, "epic button 4");
                UI.CreateButton(panel, "epic button 5");
                UI.CreateButton(panel, "epic button 6");
                UI.CreateButton(panel, "epic button 7");
                UI.CreateButton(panel, "epic button 8");
                UI.CreateButton(panel, "epic button 9");
                UI.CreateButton(panel, "wide boi", 580, 30);
            }, "Styles");

            Plugin.Log.LogInfo("Created UI");

            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        }

        private void Update() {
            shouldAscend = Input.GetKeyDown(KeyCode.Equals);
            shouldDescend = Input.GetKeyDown(KeyCode.Minus);
            if (Input.GetKeyDown(KeyCode.RightControl)) showOriginalStyle = !showOriginalStyle;
        }

        private void OnDestroy() {
            harmony?.UnpatchSelf();
            UI.Unload();
        }
    }
}
