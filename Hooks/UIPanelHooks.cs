using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;
using MelonLoader;

namespace LocationDisplay.Hooks;

public class UIPanelHooks
{
    [HarmonyPatch(typeof(Canvas), nameof(Canvas.Update))]
    public class CanvasUpdateHook
    {
        private static void Postfix(Canvas __instance)
        {
            if (__instance == null) return;
            
            // Update UI elements here if needed
            // This is where you would handle any UI updates that need to happen every frame
        }
    }

    [HarmonyPatch(typeof(TextMeshProUGUI), nameof(TextMeshProUGUI.OnEnable))]
    public class TextMeshProEnableHook
    {
        private static void Postfix(TextMeshProUGUI __instance)
        {
            if (__instance == null) return;
            
            // Handle text mesh pro enable events here
            // This is useful for ensuring your UI text elements are properly initialized
        }
    }
} 