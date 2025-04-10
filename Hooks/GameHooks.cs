using UnityEngine;
using HarmonyLib;
using MelonLoader;
using Il2CppTMPro;
using Il2CppInterop.Runtime.InteropTypes;
using Il2Cpp;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using System.Reflection;
using Il2CppInterop.Runtime.Attributes;
using System;
using Exception = System.Exception;
using System.Runtime.InteropServices;
using Assembly_CSharp;

namespace LocationDisplay.Hooks
{
    /// <summary>
    /// Static class that handles game hooks and player information retrieval.
    /// This class uses Harmony to patch game methods and provides access to player data.
    /// </summary>
    public static class GameHooks
    {
        private static PlayerCharacter localPlayer;
        private static string currentZone = "Unknown";
        private static int previousDay;
        private static int previousHour;
        private static int previousMinute;

        [HarmonyPatch(typeof(PlayerCharacter), nameof(PlayerCharacter.Update))]
        public class PlayerUpdateHook
        {
            private static void Postfix(PlayerCharacter __instance)
            {
                try
                {
                    if (__instance.IsLocalPlayer)
                    {
                        localPlayer = __instance;
                        MelonLogger.Msg("[LocationDisplay] Local player found");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[LocationDisplay] Error in PlayerUpdateHook: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(TimeController), nameof(TimeController.Update))]
        public class TimeControllerHook
        {
            private static void Postfix(TimeController __instance)
            {
                try
                {
                    previousDay = TimeController.previousDay;
                    previousHour = TimeController.previousHour;
                    previousMinute = TimeController.previousMinute;
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[LocationDisplay] Error in TimeControllerHook: {ex}");
                }
            }
        }

        /// <summary>
        /// Retrieves the current player's coordinates.
        /// </summary>
        /// <returns>A tuple containing the player's x, y, z coordinates, and direction.</returns>
        public static (float x, float y, float z, float direction) GetPlayerCoordinates()
        {
            try
            {
                if (localPlayer == null)
                {
                    return (0, 0, 0, 0);
                }

                var position = localPlayer.transform.position;
                var rotation = localPlayer.transform.rotation.eulerAngles.y;
                return (position.x, position.y, position.z, rotation);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error getting player coordinates: {ex}");
                return (0, 0, 0, 0);
            }
        }

        public static string GetCurrentZone()
        {
            try
            {
                if (localPlayer == null)
                {
                    return "Unknown";
                }

                // Get the current zone from the player's position
                var zone = ZoneManager.GetZoneAtPosition(localPlayer.transform.position);
                if (zone != null)
                {
                    currentZone = zone.ZoneName;
                }
                return currentZone;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error getting current zone: {ex}");
                return "Unknown";
            }
        }

        public static string GetCurrentTime()
        {
            try
            {
                return $"{previousDay:D2}:{previousHour:D2}:{previousMinute:D2}";
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error getting current time: {ex}");
                return "00:00:00";
            }
        }
    }
} 