using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Il2Cpp;
using MelonLoader;
using System;
using Exception = System.Exception;

namespace LocationDisplay.Hooks
{
    public static class UIHooks
    {
        private static GameObject displayPanel;
        private static Text locationText;
        private static Text timeText;
        private static Button copyButton;

        public static void InitializeUI()
        {
            try
            {
                // Get the front canvas from UIPanelRoots
                var canvas = UIPanelRoots.Instance?.Front;
                if (canvas == null)
                {
                    MelonLogger.Error("[LocationDisplay] Could not find front canvas");
                    return;
                }

                // Create the display panel
                displayPanel = new GameObject("LocationDisplayPanel");
                displayPanel.transform.SetParent(canvas.transform, false);
                displayPanel.layer = LayerMask.NameToLayer("UI");

                // Add UIWindowPanel component
                var windowPanel = displayPanel.AddComponent<UIWindowPanel>();
                windowPanel.SetTitle("Location Display");

                // Add UIDraggable component
                var draggable = displayPanel.AddComponent<UIDraggable>();
                draggable.SetDragTarget(displayPanel.GetComponent<RectTransform>());

                // Set up the panel's RectTransform
                var rectTransform = displayPanel.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(300, 100);
                rectTransform.anchoredPosition = Vector2.zero;

                // Add background image
                var background = displayPanel.AddComponent<Image>();
                background.color = new Color(0f, 0f, 0f, 0.7f);

                // Create location text
                var locationObj = new GameObject("LocationText");
                locationObj.transform.SetParent(displayPanel.transform, false);
                locationText = locationObj.AddComponent<Text>();
                locationText.color = Color.white;
                locationText.fontSize = 14;
                locationText.alignment = TextAnchor.MiddleLeft;
                locationText.rectTransform.anchorMin = new Vector2(0, 0);
                locationText.rectTransform.anchorMax = new Vector2(1, 1);
                locationText.rectTransform.offsetMin = new Vector2(10, 10);
                locationText.rectTransform.offsetMax = new Vector2(-10, -10);

                // Create time text
                var timeObj = new GameObject("TimeText");
                timeObj.transform.SetParent(displayPanel.transform, false);
                timeText = timeObj.AddComponent<Text>();
                timeText.color = Color.white;
                timeText.fontSize = 14;
                timeText.alignment = TextAnchor.MiddleRight;
                timeText.rectTransform.anchorMin = new Vector2(0, 0);
                timeText.rectTransform.anchorMax = new Vector2(1, 1);
                timeText.rectTransform.offsetMin = new Vector2(10, 10);
                timeText.rectTransform.offsetMax = new Vector2(-10, -10);

                // Create copy button
                var buttonObj = new GameObject("CopyButton");
                buttonObj.transform.SetParent(displayPanel.transform, false);
                copyButton = buttonObj.AddComponent<Button>();
                var buttonText = buttonObj.AddComponent<Text>();
                buttonText.text = "Copy";
                buttonText.color = Color.white;
                buttonText.fontSize = 12;
                buttonText.alignment = TextAnchor.MiddleCenter;

                var buttonRect = buttonObj.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.5f, 0);
                buttonRect.anchorMax = new Vector2(0.5f, 0);
                buttonRect.sizeDelta = new Vector2(80, 20);
                buttonRect.anchoredPosition = new Vector2(0, 10);

                // Set up button colors
                var colors = copyButton.colors;
                colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                copyButton.colors = colors;

                // Add click handler
                copyButton.onClick.AddListener((UnityAction)CopyLocationToClipboard);

                MelonLogger.Msg("[LocationDisplay] UI initialized successfully");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error initializing UI: {ex}");
            }
        }

        public static void UpdateDisplay(string location, string time)
        {
            try
            {
                if (locationText != null)
                {
                    locationText.text = location;
                }
                if (timeText != null)
                {
                    timeText.text = time;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error updating display: {ex}");
            }
        }

        private static void CopyLocationToClipboard()
        {
            try
            {
                var (x, y, z, direction) = GameHooks.GetPlayerCoordinates();
                var zone = GameHooks.GetCurrentZone();
                var time = GameHooks.GetCurrentTime();

                var locationString = $"Zone: {zone}\nX: {x:F2} (E/W)\nY: {y:F2} (N/S)\nZ: {z:F2} (U/D)\nTime: {time}\nDirection: {direction:F0}°";
                GUIUtility.systemCopyBuffer = locationString;
                MelonLogger.Msg("[LocationDisplay] Location copied to clipboard");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error copying location to clipboard: {ex}");
            }
        }
    }
} 