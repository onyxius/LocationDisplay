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
using System.Collections;
using Exception = System.Exception;

[assembly: MelonInfo(typeof(LocationDisplay.ModMain), "Location Display", "1.0.0", "Onyxius")]
[assembly: MelonGame("Visionary Realms", "Pantheon")]
[assembly: MelonColor(34, 139, 34, 255)] // Forest Green RGB values with alpha

namespace LocationDisplay
{
    // Main mod class that initializes the mod and sets up the UI
    public class ModMain : MelonMod
    {
        private bool isInitialized = false;
        private float initTimer = 0f;
        private const float INIT_DELAY = 2f;

        public override void OnInitializeMelon()
        {
            try
            {
                MelonLogger.Msg("[LocationDisplay] Mod initialized");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error during initialization: {ex}");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            try
            {
                MelonLogger.Msg($"[LocationDisplay] Scene loaded: {sceneName}");
                isInitialized = false;
                initTimer = 0f;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error during scene load: {ex}");
            }
        }

        public override void OnUpdate()
        {
            try
            {
                if (!isInitialized)
                {
                    initTimer += Time.deltaTime;
                    if (initTimer >= INIT_DELAY)
                    {
                        Initialize();
                    }
                    return;
                }

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error during update: {ex}");
            }
        }

        private void Initialize()
        {
            try
            {
                if (UIPanelRoots.Instance?.Front == null)
                {
                    MelonLogger.Msg("[LocationDisplay] Waiting for UI system to initialize...");
                    return;
                }

                Hooks.UIHooks.InitializeUI();
                isInitialized = true;
                MelonLogger.Msg("[LocationDisplay] UI initialized successfully");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error initializing UI: {ex}");
            }
        }

        private void UpdateDisplay()
        {
            try
            {
                var (x, y, z, direction) = Hooks.GameHooks.GetPlayerCoordinates();
                var zone = Hooks.GameHooks.GetCurrentZone();
                var time = Hooks.GameHooks.GetCurrentTime();

                var locationText = $"Zone: {zone}\nX: {x:F2} (E/W)\nY: {y:F2} (N/S)\nZ: {z:F2} (U/D)";
                var timeText = $"Time: {time}\nDirection: {direction:F0}°";

                Hooks.UIHooks.UpdateDisplay(locationText, timeText);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error updating display: {ex}");
            }
        }

        public override void OnApplicationQuit()
        {
            try
            {
                MelonLogger.Msg("[LocationDisplay] Mod shutting down");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error during shutdown: {ex}");
            }
        }
    }
} 