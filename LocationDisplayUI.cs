using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;
using System;
using System.Collections.Generic;
using MelonLoader;
using Il2Cpp;
using LocationDisplay.Hooks;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Attributes;

namespace LocationDisplay
{
    /// <summary>
    /// UI component that displays the player's current location and time in-game.
    /// This class handles the creation and management of the display elements.
    /// </summary>
    public class LocationDisplayUI : MonoBehaviour
    {
        // Static instance for easy access from other parts of the mod
        public static LocationDisplayUI Instance { get; private set; }

        // UI text components for displaying location and time
        private TextMeshProUGUI locationText;
        private TextMeshProUGUI timeText;

        /// <summary>
        /// Initializes the UI components and sets up the display.
        /// This method creates the necessary GameObjects and configures their properties.
        /// </summary>
        public void Initialize()
        {
            try
            {
                // Set the static instance for global access
                Instance = this;
                MelonLogger.Msg("[LocationDisplay] UI component initialized");

                // Create and configure the location text display
                var locationObj = new GameObject("LocationText");
                locationObj.transform.SetParent(transform, false);
                
                // Set up the RectTransform for proper positioning
                var locationRect = locationObj.AddComponent<RectTransform>();
                locationRect.anchorMin = new Vector2(0, 1);  // Anchor to top-left
                locationRect.anchorMax = new Vector2(1, 1);  // Stretch horizontally
                locationRect.pivot = new Vector2(0.5f, 1);   // Pivot at top-center
                locationRect.anchoredPosition = new Vector2(0, 0);
                locationRect.sizeDelta = new Vector2(0, 30); // Fixed height

                // Add and configure the TextMeshProUGUI component
                locationText = locationObj.AddComponent<TextMeshProUGUI>();
                if (locationText != null)
                {
                    locationText.fontSize = 16;
                    locationText.alignment = TextAlignmentOptions.Left;
                    locationText.text = "Location: Unknown";
                    locationText.color = Color.white;
                    locationText.enableWordWrapping = false;
                    locationText.overflowMode = TextOverflowModes.Truncate;
                }
                else
                {
                    MelonLogger.Error("[LocationDisplay] Failed to create location text component");
                    return;
                }

                // Create and configure the time text display
                var timeObj = new GameObject("TimeText");
                timeObj.transform.SetParent(transform, false);
                
                // Set up the RectTransform for proper positioning
                var timeRect = timeObj.AddComponent<RectTransform>();
                timeRect.anchorMin = new Vector2(0, 1);      // Anchor to top-left
                timeRect.anchorMax = new Vector2(1, 1);      // Stretch horizontally
                timeRect.pivot = new Vector2(0.5f, 1);       // Pivot at top-center
                timeRect.anchoredPosition = new Vector2(0, -30); // Position below location text
                timeRect.sizeDelta = new Vector2(0, 30);     // Fixed height

                // Add and configure the TextMeshProUGUI component
                timeText = timeObj.AddComponent<TextMeshProUGUI>();
                if (timeText != null)
                {
                    timeText.fontSize = 16;
                    timeText.alignment = TextAlignmentOptions.Left;
                    timeText.text = "Time: 00:00";
                    timeText.color = Color.white;
                    timeText.enableWordWrapping = false;
                    timeText.overflowMode = TextOverflowModes.Truncate;
                }
                else
                {
                    MelonLogger.Error("[LocationDisplay] Failed to create time text component");
                    return;
                }

                // Create and configure the background panel
                var bgObj = new GameObject("Background");
                bgObj.transform.SetParent(transform, false);
                bgObj.transform.SetAsFirstSibling(); // Ensure background is behind text
                
                // Set up the RectTransform to cover the entire display area
                var bgRect = bgObj.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;  // Anchor to bottom-left
                bgRect.anchorMax = Vector2.one;   // Anchor to top-right
                bgRect.sizeDelta = Vector2.zero;  // Fill the entire space

                // Add and configure the background image
                var bgImage = bgObj.AddComponent<Image>();
                bgImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black background

                MelonLogger.Msg("[LocationDisplay] UI elements created and configured");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in Initialize: {ex}");
            }
        }

        /// <summary>
        /// Updates the displayed player information with current location and time.
        /// This method is called periodically to keep the display current.
        /// </summary>
        public void UpdatePlayerInfo()
        {
            try
            {
                // Get current player information from GameHooks
                var (zone, time) = GameHooks.GetPlayerInfo();

                // Update the location text if the component exists
                if (locationText != null)
                {
                    locationText.text = $"Location: {zone}";
                }

                // Update the time text if the component exists
                if (timeText != null)
                {
                    timeText.text = $"Time: {time}";
                }

                MelonLogger.Msg($"[LocationDisplay] UI updated - Zone: {zone}, Time: {time}");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in UpdatePlayerInfo: {ex}");
            }
        }

        /// <summary>
        /// Called when the component is destroyed.
        /// Cleans up the static instance to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            try
            {
                // Clean up the static instance if it points to this object
                if (Instance == this)
                {
                    Instance = null;
                }

                MelonLogger.Msg("[LocationDisplay] UI component destroyed");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error in OnDestroy: {ex}");
            }
        }
    }
} 