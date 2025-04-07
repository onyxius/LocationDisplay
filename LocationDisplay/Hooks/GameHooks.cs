using HarmonyLib;
using UnityEngine;
using Il2Cpp;
using Il2CppScripts;

namespace LocationDisplay.Hooks;

public class GameHooks
{
    private static string currentZone = "Unknown";
    private static string currentTime = "00:00";

    [HarmonyPatch(typeof(Il2CppScripts.Player), nameof(Il2CppScripts.Player.Update))]
    public class PlayerUpdateHook
    {
        private static void Postfix(Il2CppScripts.Player __instance)
        {
            if (__instance == null) return;

            // Get current zone name
            if (ZoneManager.Instance != null)
            {
                currentZone = ZoneManager.Instance.CurrentZone?.Name ?? "Unknown";
            }

            // Get current time
            if (TimeManager.Instance != null)
            {
                var time = TimeManager.Instance.CurrentTime;
                currentTime = $"{time.Hours:D2}:{time.Minutes:D2}";
            }
        }
    }

    public static string GetCurrentZone() => currentZone;
    public static string GetCurrentTime() => currentTime;
} 