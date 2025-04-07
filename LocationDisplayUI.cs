using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;
using MelonLoader;
using System;
using LocationDisplay.Hooks;

namespace LocationDisplay
{
    public class LocationDisplayUI : MonoBehaviour
    {
        private static LocationDisplayUI instance;
        private TextMeshProUGUI displayText;
        private RectTransform rectTransform;
        private bool isDragging;
        private Vector2 dragOffset;

        public static LocationDisplayUI CreateUI(Transform parent)
        {
            try
            {
                if (instance != null)
                {
                    MelonLogger.Warning("LocationDisplayUI instance already exists!");
                    return instance;
                }

                MelonLogger.Msg("Creating LocationDisplayUI...");
                var go = new GameObject("LocationDisplay");
                go.transform.SetParent(parent, false);
                instance = go.AddComponent<LocationDisplayUI>();
                return instance;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to create LocationDisplayUI: {ex}");
                return null;
            }
        }

        private void Awake()
        {
            try
            {
                MelonLogger.Msg("LocationDisplayUI Awake - Initializing UI elements...");
                InitializeUI();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in LocationDisplayUI.Awake: {ex}");
            }
        }

        private void InitializeUI()
        {
            // Setup RectTransform
            rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = gameObject.AddComponent<RectTransform>();

            rectTransform.sizeDelta = new Vector2(200, 60);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchoredPosition = new Vector2(10, 10);

            // Add background image
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.5f);
            background.raycastTarget = true;

            // Add text component
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(transform, false);

            displayText = textObj.AddComponent<TextMeshProUGUI>();
            displayText.fontSize = 16;
            displayText.alignment = TextAlignmentOptions.Left;
            displayText.color = Color.white;
            displayText.text = "Zone: Loading...\nTime: --:--";

            var textRect = displayText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);

            MelonLogger.Msg("UI elements initialized successfully");
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;

            try
            {
                UpdateDisplay();
                HandleDragging();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in LocationDisplayUI.Update: {ex}");
            }
        }

        private void UpdateDisplay()
        {
            if (displayText == null)
            {
                MelonLogger.Error("DisplayText component is null!");
                return;
            }

            try
            {
                string zone = GameHooks.GetCurrentZone();
                string time = GameHooks.GetCurrentTime();
                string newText = $"Zone: {zone}\nTime: {time}";

                if (displayText.text != newText)
                {
                    displayText.text = newText;
                    MelonLogger.Msg($"Display updated - Zone: {zone}, Time: {time}");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error updating display: {ex}");
            }
        }

        private void HandleDragging()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos))
                {
                    isDragging = true;
                    dragOffset = rectTransform.anchoredPosition - (Vector2)Input.mousePosition;
                    MelonLogger.Msg("Started dragging UI");
                }
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                MelonLogger.Msg($"Stopped dragging UI - Position: {rectTransform.anchoredPosition}");
            }

            if (isDragging)
            {
                rectTransform.anchoredPosition = (Vector2)Input.mousePosition + dragOffset;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                MelonLogger.Msg("LocationDisplayUI instance destroyed");
            }
        }
    }
} 