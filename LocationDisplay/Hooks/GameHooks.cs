using HarmonyLib;
using UnityEngine;
using Il2Cpp;

namespace LocationDisplay.Hooks;

public class GameHooks
{
    private static string currentZone = "Unknown";
    private static string currentTime = "00:00";

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    public class PlayerUpdateHook
    {
        private static void Postfix(Player __instance)
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