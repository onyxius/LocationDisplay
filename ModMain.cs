using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;
using System.Runtime.InteropServices;
using System;
using UnityEngine.SceneManagement;
using HarmonyLib;
using Il2Cpp;
using System.Linq;
using LocationDisplay.Hooks;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using System.Reflection;

[assembly: MelonInfo(typeof(LocationDisplay.ModMain), "Location Display", "1.0.0", "Onyxius")]
[assembly: MelonGame("Visionary Realms", "Pantheon")]
[assembly: MelonColor(34, 139, 34, 255)] // Forest Green RGB values with alpha

namespace LocationDisplay
{
    // Main mod class that initializes the mod and sets up the UI
    public class ModMain : MelonMod
    {
        // Static instance for easy access to the mod's main functionality
        public static ModMain Instance { get; private set; }

        // Reference to the UI component
        private LocationDisplayUI displayUI;

        // Harmony instance for patching
        private HarmonyLib.Harmony harmony;

        // Flag to track if the display has been initialized
        private bool hasInitialized = false;

        // Called when the mod is initialized
        public override void OnInitializeMelon()
        {
            try
            {
                Instance = this;
                
                // Initialize Harmony
                harmony = new HarmonyLib.Harmony("com.onyxi.locationdisplay");
                
                // Apply Harmony patches
                var originalNetworkStart = AccessTools.Method(typeof(EntityPlayerGameObject), "NetworkStart");
                var postfixNetworkStart = typeof(Hooks.GameHooks.PlayerStartHook).GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static);
                
                if (originalNetworkStart != null && postfixNetworkStart != null)
                {
                    harmony.Patch(originalNetworkStart, postfix: new HarmonyMethod(postfixNetworkStart));
                    MelonLogger.Msg("[LocationDisplay] Applied NetworkStart patch");
                }
                else
                {
                    MelonLogger.Error($"[LocationDisplay] Failed to apply NetworkStart patch. Original: {originalNetworkStart != null}, Postfix: {postfixNetworkStart != null}");
                }

                var originalOnDestroy = AccessTools.Method(typeof(EntityPlayerGameObject), "OnDestroy");
                var postfixOnDestroy = typeof(Hooks.GameHooks.PlayerDestroyHook).GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static);
                
                if (originalOnDestroy != null && postfixOnDestroy != null)
                {
                    harmony.Patch(originalOnDestroy, postfix: new HarmonyMethod(postfixOnDestroy));
                    MelonLogger.Msg("[LocationDisplay] Applied OnDestroy patch");
                }
                else
                {
                    MelonLogger.Error($"[LocationDisplay] Failed to apply OnDestroy patch. Original: {originalOnDestroy != null}, Postfix: {postfixOnDestroy != null}");
                }

                MelonLogger.Msg("[LocationDisplay] Mod initialized successfully");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error during initialization: {ex}");
            }
        }

        // Called when the mod is loaded into the game
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            try
            {
                MelonLogger.Msg($"[LocationDisplay] Scene loaded: {sceneName}");
                
                // Only initialize in gameplay scenes
                if (sceneName == "Gameplay")
                {
                    MelonLogger.Msg("[LocationDisplay] Gameplay scene detected, initializing display");
                    InitializeDisplay();
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in OnSceneWasLoaded: {ex}");
            }
        }

        // Called when the mod is updated
        public override void OnUpdate()
        {
            try
            {
                if (!hasInitialized)
                {
                    // Try to initialize if we haven't yet
                    InitializeDisplay();
                }
                else if (displayUI != null)
                {
                    // Update the display if we have it
                    displayUI.UpdatePlayerInfo();
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in OnUpdate: {ex}");
            }
        }

        // Sets up the UI display by finding or creating the necessary GameObjects
        private void InitializeDisplay()
        {
            try
            {
                // Find the XPWindow which will be our parent
                var xpWindow = GameObject.Find("XPWindow");
                if (xpWindow == null)
                {
                    return; // Silently return, we'll try again next frame
                }

                // Create our display object
                var displayObject = new GameObject("LocationDisplay");
                displayObject.transform.SetParent(xpWindow.transform, false);

                // Set up RectTransform for proper UI positioning
                var rectTransform = displayObject.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.anchoredPosition = new Vector2(10, -10);
                rectTransform.sizeDelta = new Vector2(200, 60);

                // Add Canvas component
                var canvas = displayObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000; // Ensure it's visible

                // Add CanvasScaler
                var scaler = displayObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // Add our UI component
                displayUI = displayObject.AddComponent<LocationDisplayUI>();
                if (displayUI != null)
                {
                    displayUI.Initialize();
                    hasInitialized = true;
                    MelonLogger.Msg("[LocationDisplay] Display initialized successfully");
                }
                else
                {
                    MelonLogger.Error("[LocationDisplay] Failed to create LocationDisplayUI component");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in InitializeDisplay: {ex}");
            }
        }

        // Called when the mod is unloaded
        public override void OnDeinitializeMelon()
        {
            try
            {
                MelonLogger.Msg("[LocationDisplay] Deinitializing Location Display mod...");
                
                // Clean up Harmony patches
                if (harmony != null)
                {
                    harmony.UnpatchSelf();
                    MelonLogger.Msg("[LocationDisplay] Harmony patches removed");
                }
                
                // Clean up UI
                if (displayUI != null)
                {
                    GameObject.Destroy(displayUI.gameObject);
                    displayUI = null;
                    MelonLogger.Msg("[LocationDisplay] UI cleaned up");
                }
                
                hasInitialized = false;
                MelonLogger.Msg("[LocationDisplay] Location Display mod deinitialized successfully");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in OnDeinitializeMelon: {ex}");
            }
        }

        public override void OnApplicationQuit()
        {
            try
            {
                MelonLogger.Msg("[LocationDisplay] Application quitting...");
                OnDeinitializeMelon();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in OnApplicationQuit: {ex}");
            }
        }
    }
} 