using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Il2CppTMPro;
using MelonLoader;
using Il2CppInterop.Runtime;
using Il2Cpp;

namespace LocationDisplay.Hooks
{
    public static class UIHooks
    {
        private static GameObject displayPanel;
        private static TextMeshProUGUI displayText;
        private static RectTransform displayRect;
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (isInitialized) return;

            try
            {
                // Create canvas if it doesn't exist
                var canvasObj = new GameObject("LocationDisplayCanvas");
                var canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999; // Ensure it's on top
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                UnityEngine.Object.DontDestroyOnLoad(canvasObj);

                // Create panel
                displayPanel = new GameObject("LocationDisplayPanel");
                displayPanel.transform.SetParent(canvasObj.transform, false);

                // Add RectTransform
                displayRect = displayPanel.AddComponent<RectTransform>();
                displayRect.sizeDelta = new Vector2(300, 60);

                // Set anchoring (bottom-left)
                displayRect.anchorMin = new Vector2(0, 0);
                displayRect.anchorMax = new Vector2(0, 0);
                displayRect.pivot = new Vector2(0, 0);
                displayRect.anchoredPosition = new Vector2(ModMain.xPosition, ModMain.yPosition);

                // Add background image and make it draggable
                var image = displayPanel.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 0.5f);
                
                // Add window resize handle component
                var windowHandle = displayPanel.AddComponent<WindowResizeHandle>();
                windowHandle.Initialize(displayRect);

                // Create text object
                var textObj = new GameObject("LocationDisplayText");
                textObj.transform.SetParent(displayPanel.transform, false);

                // Setup text
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                // Create TextMeshProUGUI using Il2Cpp interop
                var tmpType = Il2CppType.Of<TextMeshProUGUI>();
                displayText = textObj.AddComponent(tmpType).Cast<TextMeshProUGUI>();
                displayText.alignment = TextAlignmentOptions.Center;
                displayText.fontSize = ModMain.Config.GetEntry<int>("fontSize").Value;
                displayText.color = Color.white;

                isInitialized = true;
                MelonLogger.Msg("Successfully created location display");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error creating display: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static void UpdateDisplay(string location, string time)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            try
            {
                if (displayText != null)
                {
                    displayText.text = $"Location: {location}\nTime: {time}";
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error updating display: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    // Window resize/drag handle component based on the Quest Progress Tracker implementation
    public class WindowResizeHandle : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Vector2 dragStartPosition;
        private Vector2 dragStartMousePosition;
        private bool isDragging;

        public void Initialize(RectTransform rect)
        {
            rectTransform = rect;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                // Check if mouse is over the panel
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, UnityEngine.Input.mousePosition))
                {
                    StartDragging();
                }
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                StopDragging();
            }

            if (isDragging)
            {
                UpdateDragging();
            }
        }

        private void StartDragging()
        {
            isDragging = true;
            dragStartPosition = rectTransform.anchoredPosition;
            dragStartMousePosition = UnityEngine.Input.mousePosition;
        }

        private void StopDragging()
        {
            if (isDragging)
            {
                isDragging = false;
                // Save the new position
                ModMain.xPosition = rectTransform.anchoredPosition.x;
                ModMain.yPosition = rectTransform.anchoredPosition.y;
                ModMain.Config.GetEntry<float>("xPosition").Value = ModMain.xPosition;
                ModMain.Config.GetEntry<float>("yPosition").Value = ModMain.yPosition;
                ModMain.SaveConfig();
            }
        }

        private void UpdateDragging()
        {
            if (rectTransform == null) return;

            Vector2 currentMousePosition = UnityEngine.Input.mousePosition;
            Vector2 difference = currentMousePosition - dragStartMousePosition;

            rectTransform.anchoredPosition = dragStartPosition + difference;
        }
    }
} 