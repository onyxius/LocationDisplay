using UnityEngine;
using HarmonyLib;
using MelonLoader;
using System;
using Il2CppTMPro;
using Il2CppInterop.Runtime.InteropTypes;
using Il2Cpp;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using System.Reflection;
using Il2CppInterop.Runtime.Attributes;

namespace LocationDisplay.Hooks
{
    /// <summary>
    /// Static class that handles game hooks and player information retrieval.
    /// This class uses Harmony to patch game methods and provides access to player data.
    /// </summary>
    public static class GameHooks
    {
        // Static instance of the Harmony patcher
        private static Harmony harmony;

        // Reference to the local player object
        private static EntityPlayerGameObject localPlayer;

        // Flag to track if hooks have been initialized
        private static bool hooksInitialized = false;

        /// <summary>
        /// Initializes the Harmony hooks for the mod.
        /// This method should be called once when the mod starts.
        /// </summary>
        public static void InitializeHooks()
        {
            try
            {
                if (hooksInitialized) return;

                // Create a new Harmony instance with a unique ID
                harmony = new Harmony("com.locationdisplay.patches");
                
                // Apply the patches to the game methods
                harmony.PatchAll();
                
                hooksInitialized = true;
                MelonLogger.Msg("[LocationDisplay] Hooks initialized successfully");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Failed to initialize hooks: {ex}");
            }
        }

        /// <summary>
        /// Harmony patch for the player's NetworkStart method.
        /// This is called when a player object is created in the game.
        /// </summary>
        [HarmonyPatch(typeof(EntityPlayerGameObject), "NetworkStart")]
        private static class PlayerNetworkStart
        {
            /// <summary>
            /// Postfix method that runs after the original NetworkStart.
            /// Used to identify and store the local player reference.
            /// </summary>
            [HarmonyPostfix]
            private static void Postfix(EntityPlayerGameObject __instance)
            {
                try
                {
                    // Get the NetworkId field using reflection
                    var networkIdField = typeof(EntityPlayerGameObject).GetField("NetworkId", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (networkIdField == null)
                    {
                        MelonLogger.Error("[LocationDisplay] Failed to find NetworkId field");
                        return;
                    }

                    // Get the local player ID field
                    var localPlayerIdField = typeof(EntityPlayerGameObject).GetField("LocalPlayerId", BindingFlags.Static | BindingFlags.NonPublic);
                    if (localPlayerIdField == null)
                    {
                        MelonLogger.Error("[LocationDisplay] Failed to find LocalPlayerId field");
                        return;
                    }

                    // Get the current values
                    int networkId = (int)networkIdField.GetValue(__instance);
                    int localPlayerId = (int)localPlayerIdField.GetValue(null);

                    MelonLogger.Msg($"[LocationDisplay] Player spawned - NetworkId: {networkId}, LocalPlayerId: {localPlayerId}");

                    // Check if this is the local player
                    if (networkId == localPlayerId)
                    {
                        localPlayer = __instance;
                        MelonLogger.Msg("[LocationDisplay] Local player set");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[LocationDisplay] Error in PlayerNetworkStart: {ex}");
                }
            }
        }

        /// <summary>
        /// Harmony patch for the player's OnDestroy method.
        /// This is called when a player object is destroyed in the game.
        /// </summary>
        [HarmonyPatch(typeof(EntityPlayerGameObject), "OnDestroy")]
        private static class PlayerDestroyHook
        {
            /// <summary>
            /// Postfix method that runs after the original OnDestroy.
            /// Used to clear the local player reference when the player is destroyed.
            /// </summary>
            [HarmonyPostfix]
            private static void Postfix(EntityPlayerGameObject __instance)
            {
                try
                {
                    // Check if the destroyed player is the local player
                    if (localPlayer == __instance)
                    {
                        localPlayer = null;
                        MelonLogger.Msg("[LocationDisplay] Local player cleared");
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[LocationDisplay] Error in PlayerDestroyHook: {ex}");
                }
            }
        }

        /// <summary>
        /// Retrieves the current player's location and time information.
        /// </summary>
        /// <returns>A tuple containing the zone name and current time.</returns>
        public static (string zone, string time) GetPlayerInfo()
        {
            try
            {
                // Default values if information cannot be retrieved
                string zone = "Unknown";
                string time = "00:00";

                // Check if we have a valid local player reference
                if (localPlayer != null)
                {
                    // Get the current zone from the player's position
                    zone = GetCurrentZone();
                    
                    // Get the current game time
                    time = GetCurrentTime();
                }

                return (zone, time);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in GetPlayerInfo: {ex}");
                return ("Error", "Error");
            }
        }

        /// <summary>
        /// Determines the current zone based on the player's position.
        /// </summary>
        /// <returns>The name of the current zone.</returns>
        private static string GetCurrentZone()
        {
            try
            {
                // Get the ZoneManager instance
                var zoneManager = GameObject.Find("ZoneManager");
                if (zoneManager == null)
                {
                    MelonLogger.Error("[LocationDisplay] ZoneManager not found");
                    return "Unknown";
                }

                // Get the current zone component
                var currentZone = zoneManager.GetComponent<Zone>();
                if (currentZone == null)
                {
                    MelonLogger.Error("[LocationDisplay] CurrentZone component not found");
                    return "Unknown";
                }

                return currentZone.ZoneName;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in GetCurrentZone: {ex}");
                return "Unknown";
            }
        }

        /// <summary>
        /// Retrieves the current game time.
        /// </summary>
        /// <returns>The current time formatted as HH:MM.</returns>
        private static string GetCurrentTime()
        {
            try
            {
                // Get the TimeDisplay instance
                var timeDisplay = GameObject.Find("TimeDisplay");
                if (timeDisplay == null)
                {
                    MelonLogger.Error("[LocationDisplay] TimeDisplay not found");
                    return "00:00";
                }

                // Get the current time component
                var currentTime = timeDisplay.GetComponent<TimeDisplay>();
                if (currentTime == null)
                {
                    MelonLogger.Error("[LocationDisplay] CurrentTime component not found");
                    return "00:00";
                }

                return currentTime.GetFormattedTime();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in GetCurrentTime: {ex}");
                return "00:00";
            }
        }

        public static EntityPlayerGameObject GetLocalPlayer() => localPlayer;

        public static void OnPlayerSpawned(EntityPlayerGameObject player)
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
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in OnPlayerSpawned: {ex}");
            }
        }

        public static void OnPlayerDespawned(EntityPlayerGameObject player)
        {
            try
            {
                if (player == null)
                {
                    MelonLogger.Msg("Player despawned with null instance");
                    return;
                }
                MelonLogger.Msg("Player despawned");
                if (ReferenceEquals(localPlayer, player))
                {
                    localPlayer = null;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in OnPlayerDespawned: {ex}");
            }
        }
    }
} 