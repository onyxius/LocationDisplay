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

[assembly: MelonInfo(typeof(LocationDisplay.ModMain), "Location Display", "1.0.0", "Onyxi")]
[assembly: MelonGame("Visionary Realms", "Pantheon")]
[assembly: MelonColor(34, 139, 34, 255)] // Forest Green RGB values with alpha

namespace LocationDisplay
{
    public class ModMain : MelonMod
    {
        public static MelonPreferences_Category Config { get; private set; }
        private static GameObject displayObject;
        private static LocationDisplayUI locationDisplay;
        private static HarmonyLib.Harmony harmony;
        private static bool isInitialized = false;

        public override void OnInitializeMelon()
        {
            try
            {
                MelonLogger.Msg("Initializing Location Display mod...");
                
                // Create preferences
                Config = MelonPreferences.CreateCategory("LocationDisplay");
                MelonLogger.Msg("Created preferences category");
                
                // Initialize Harmony patches
                harmony = new HarmonyLib.Harmony("com.onyxi.locationdisplay");
                try
                {
                    harmony.PatchAll();
                    MelonLogger.Msg("Applied Harmony patches");
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to apply Harmony patches: {ex}");
                    return;
                }

                // Subscribe to scene loading events
                SceneManager.sceneLoaded += OnSceneLoaded;
                MelonLogger.Msg("Subscribed to scene loading events");

                MelonLogger.Msg("Location Display mod initialized successfully");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in OnInitializeMelon: {ex}");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            try
            {
                MelonLogger.Msg($"Scene loaded: {scene.name} (build index: {scene.buildIndex})");
                
                // Only initialize the UI when we're in a game scene
                if (scene.name.Contains("Game"))
                {
                    MelonLogger.Msg("Game scene detected, initializing display...");
                    if (!isInitialized)
                    {
                        InitializeDisplay();
                        isInitialized = true;
                    }
                    else
                    {
                        MelonLogger.Msg("Display already initialized");
                    }
                }
                else
                {
                    MelonLogger.Msg($"Not a game scene ({scene.name}), skipping initialization");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in OnSceneLoaded: {ex}");
            }
        }

        private void InitializeDisplay()
        {
            try
            {
                MelonLogger.Msg("Starting display initialization...");

                // Find the main canvas
                var allCanvases = GameObject.FindObjectsOfType<Canvas>();
                MelonLogger.Msg($"Found {allCanvases.Length} canvases in the scene");

                var mainCanvas = GameObject.Find("Canvas");
                if (mainCanvas == null)
                {
                    MelonLogger.Error("Could not find main canvas! Available canvases:");
                    foreach (var canvas in allCanvases)
                    {
                        MelonLogger.Msg($"Canvas: {canvas.name}");
                    }
                    return;
                }

                MelonLogger.Msg($"Found main canvas: {mainCanvas.name}");

                // Create our display object as a child of the main canvas
                displayObject = new GameObject("LocationDisplay");
                if (displayObject == null)
                {
                    MelonLogger.Error("Failed to create LocationDisplay GameObject");
                    return;
                }
                
                displayObject.transform.SetParent(mainCanvas.transform, false);
                MelonLogger.Msg("Created LocationDisplay GameObject");
                
                // Add our UI component
                try
                {
                    locationDisplay = LocationDisplayUI.CreateUI(displayObject.transform);
                    if (locationDisplay == null)
                    {
                        MelonLogger.Error("Failed to create LocationDisplayUI!");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error creating LocationDisplayUI: {ex}");
                    return;
                }
                
                MelonLogger.Msg("Display initialized successfully");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error initializing display: {ex}");
            }
        }

        public override void OnDeinitializeMelon()
        {
            try
            {
                MelonLogger.Msg("Deinitializing Location Display mod...");
                
                // Unsubscribe from scene loading events
                SceneManager.sceneLoaded -= OnSceneLoaded;
                MelonLogger.Msg("Unsubscribed from scene loading events");
                
                // Clean up Harmony patches
                if (harmony != null)
                {
                    try
                    {
                        harmony.UnpatchSelf();
                        MelonLogger.Msg("Harmony patches removed");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Error removing Harmony patches: {ex}");
                    }
                }
                
                // Clean up UI
                if (displayObject != null)
                {
                    try
                    {
                        UnityEngine.Object.Destroy(displayObject);
                        MelonLogger.Msg("Display object destroyed");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Error destroying display object: {ex}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in OnDeinitializeMelon: {ex}");
            }
        }

        public override void OnApplicationQuit()
        {
            try
            {
                MelonLogger.Msg("Application quitting...");
                if (harmony != null)
                {
                    try
                    {
                        harmony.UnpatchSelf();
                        MelonLogger.Msg("Harmony patches removed");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Error removing Harmony patches: {ex}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error in OnApplicationQuit: {ex}");
            }
        }

        public void ShowDisplay()
        {
            try
            {
                if (locationDisplay != null)
                {
                    locationDisplay.gameObject.SetActive(true);
                    MelonLogger.Msg("Display shown");
                }
                else
                {
                    MelonLogger.Msg("Cannot show display - locationDisplay is null");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error showing display: {ex}");
            }
        }

        public void HideDisplay()
        {
            try
            {
                if (locationDisplay != null)
                {
                    locationDisplay.gameObject.SetActive(false);
                    MelonLogger.Msg("Display hidden");
                }
                else
                {
                    MelonLogger.Msg("Cannot hide display - locationDisplay is null");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error hiding display: {ex}");
            }
        }
    }
} 