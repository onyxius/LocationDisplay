using UnityEngine;
using HarmonyLib;
using MelonLoader;
using System;
using Il2CppTMPro;
using Il2CppInterop.Runtime.InteropTypes;
using Il2Cpp;

namespace LocationDisplay.Hooks
{
    public static class GameHooks
    {
        private static string currentZone = "Unknown";
        private static string currentTime = "00:00";
        private static Il2CppObjectBase localPlayer;

        [HarmonyPatch(typeof(Il2CppScripts.Player), "Update")]
        public class PlayerUpdateHook
        {
            private static void Postfix(Il2CppScripts.Player __instance)
            {
                try
                {
                    if (__instance == null)
                    {
                        MelonLogger.Msg("Player instance is null in Update");
                        return;
                    }
                    localPlayer = __instance;

                    // Get current zone name
                    var zoneManager = GameObject.Find("ZoneManager");
                    if (zoneManager != null)
                    {
                        var zoneName = zoneManager.GetComponent<TextMeshProUGUI>();
                        if (zoneName != null)
                        {
                            currentZone = zoneName.text;
                            MelonLogger.Msg($"Zone updated: {currentZone}");
                        }
                        else
                        {
                            MelonLogger.Msg("ZoneManager found but no TextMeshProUGUI component");
                        }
                    }
                    else
                    {
                        MelonLogger.Msg("ZoneManager not found");
                    }

                    // Get current time
                    var timeDisplay = GameObject.Find("TimeDisplay");
                    if (timeDisplay != null)
                    {
                        var timeText = timeDisplay.GetComponent<TextMeshProUGUI>();
                        if (timeText != null)
                        {
                            currentTime = timeText.text;
                            MelonLogger.Msg($"Time updated: {currentTime}");
                        }
                        else
                        {
                            MelonLogger.Msg("TimeDisplay found but no TextMeshProUGUI component");
                        }
                    }
                    else
                    {
                        MelonLogger.Msg("TimeDisplay not found");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in PlayerUpdateHook: {ex}");
                }
            }
        }

        [HarmonyPatch(typeof(Il2CppScripts.Player), "OnDestroy")]
        public class PlayerDestroyHook
        {
            private static void Postfix(Il2CppScripts.Player __instance)
            {
                try
                {
                    if (__instance == localPlayer)
                    {
                        localPlayer = null;
                        MelonLogger.Msg("Player destroyed");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error in PlayerDestroyHook: {ex}");
                }
            }
        }

        public static string GetCurrentZone() => currentZone;
        public static string GetCurrentTime() => currentTime;
        public static Il2CppObjectBase GetLocalPlayer() => localPlayer;

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            try
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    MelonLogger.Msg("Scene name is null or empty");
                    return;
                }

                MelonLogger.Msg($"Scene loaded: {sceneName} (build index: {buildIndex})");
                OnSceneWasLoadedInternal();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in OnSceneWasLoaded: {ex}");
            }
        }

        private static void OnSceneWasLoadedInternal()
        {
            try
            {
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (!activeScene.IsValid())
                {
                    MelonLogger.Msg("Invalid active scene");
                    return;
                }

                MelonLogger.Msg($"Active scene: {activeScene.name}");

                var player = GameObject.Find("Player");
                if (player == null)
                {
                    player = GameObject.Find("LocalPlayer");
                    MelonLogger.Msg("Player not found, trying LocalPlayer");
                }

                if (player != null)
                {
                    localPlayer = player.GetComponent<Il2CppScripts.Player>();
                    if (localPlayer != null)
                    {
                        MelonLogger.Msg("Player found in scene");
                    }
                    else
                    {
                        MelonLogger.Msg("Player GameObject found but no Player component");
                    }
                }
                else
                {
                    MelonLogger.Msg("No player found in scene");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in OnSceneWasLoadedInternal: {ex}");
            }
        }

        public static void OnPlayerSpawned(Il2CppObjectBase player)
        {
            try
            {
                if (player == null)
                {
                    MelonLogger.Msg("Player spawned with null instance");
                    return;
                }
                MelonLogger.Msg("Player spawned");
                localPlayer = player;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in OnPlayerSpawned: {ex}");
            }
        }

        public static void OnPlayerDespawned(Il2CppObjectBase player)
        {
            try
            {
                if (player == null)
                {
                    MelonLogger.Msg("Player despawned with null instance");
                    return;
                }
                MelonLogger.Msg("Player despawned");
                if (localPlayer == player)
                {
                    localPlayer = null;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in OnPlayerDespawned: {ex}");
            }
        }
    }
} 