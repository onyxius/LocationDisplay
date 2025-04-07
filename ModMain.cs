using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Il2CppTMPro;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;

[assembly: MelonInfo(typeof(CoordinatesDisplay.ModMain), "Coordinates Display", "1.0.0", "Onyxi")]
[assembly: MelonGame("Visionary Realms", "Pantheon")]
[assembly: MelonColor(34, 139, 34, 255)] // Forest Green RGB values with alpha

namespace CoordinatesDisplay
{
    public class ModMain : MelonMod
    {
        private static HarmonyLib.Harmony harmony;
        public static MelonPreferences_Category Config { get; private set; }
        private static GameObject displayCanvas;
        private static GameObject displayPanel;
        private static TextMeshProUGUI displayText;
        private static RectTransform displayRect;
        private static bool isDragging;
        private static Vector2 dragStartPosition;
        private static Vector2 dragStartMousePosition;

        public override void OnInitializeMelon()
        {
            try
            {
                // Register our custom types with Il2Cpp
                ClassInjector.RegisterTypeInIl2Cpp<WindowDragHandler>();
                ClassInjector.RegisterTypeInIl2Cpp<HoverHandler>();

                // Initialize preferences
                Config = MelonPreferences.CreateCategory("Coordinates_Display");
                Config.CreateEntry("xPos", 10f);
                Config.CreateEntry("yPos", 10f);
                Config.CreateEntry("fontSize", 14);

                // Initialize Harmony
                harmony = new HarmonyLib.Harmony("com.onyxi.coordinatesdisplay");
                harmony.PatchAll(typeof(ModMain).Assembly);

                // Clean up any existing UI
                CleanupUI();

                // Create UI
                CreateUI();

                LoggerInstance.Msg("Coordinates Display mod initialized!");
            }
            catch (System.Exception ex)
            {
                LoggerInstance.Error($"Failed to initialize mod: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void CleanupUI()
        {
            var existingCanvas = GameObject.Find("CoordinatesDisplayCanvas");
            if (existingCanvas != null)
            {
                GameObject.Destroy(existingCanvas);
            }
        }

        private void CreateUI()
        {
            // Create canvas
            displayCanvas = new GameObject("CoordinatesDisplayCanvas");
            var canvas = displayCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = displayCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            displayCanvas.AddComponent<GraphicRaycaster>();
            UnityEngine.Object.DontDestroyOnLoad(displayCanvas);

            // Create panel
            displayPanel = new GameObject("CoordinatesDisplayPanel");
            displayPanel.transform.SetParent(displayCanvas.transform, false);

            // Add background image
            var image = displayPanel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.5f);

            // Setup RectTransform
            displayRect = displayPanel.GetComponent<RectTransform>();
            displayRect.sizeDelta = new Vector2(300, 80);
            displayRect.anchorMin = Vector2.zero;
            displayRect.anchorMax = Vector2.zero;
            displayRect.pivot = Vector2.zero;

            // Load saved position
            float xPos = Config.GetEntry<float>("xPos").Value;
            float yPos = Config.GetEntry<float>("yPos").Value;
            displayRect.anchoredPosition = new Vector2(xPos, yPos);

            // Create text
            var textObj = new GameObject("CoordinatesDisplayText");
            textObj.transform.SetParent(displayPanel.transform, false);

            var textRect = textObj.GetComponent<RectTransform>();
            if (textRect == null) textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);

            displayText = textObj.AddComponent<TextMeshProUGUI>();
            displayText.fontSize = Config.GetEntry<int>("fontSize").Value;
            displayText.color = Color.white;
            displayText.alignment = TextAlignmentOptions.Left;
            displayText.enableWordWrapping = false;
            displayText.overflowMode = TextOverflowModes.Overflow;

            // Ensure we have an event system
            if (GameObject.Find("EventSystem") == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                UnityEngine.Object.DontDestroyOnLoad(eventSystem);
            }
        }

        public override void OnUpdate()
        {
            try
            {
                // Handle UI interactions
                HandleUIInteractions();

                // Update display
                var player = GameObject.Find("LocalPlayer");
                if (player == null)
                {
                    UpdateDisplay("Location: Unknown\nTime: 00:00");
                    return;
                }

                var position = player.transform.position;
                var rotation = player.transform.eulerAngles.y;
                var cardinal = GetCardinalDirection(rotation);

                var timeText = GameObject.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
                var timeStr = timeText != null ? timeText.text : "00:00";

                var displayStr = $"Time: {timeStr}\n" +
                               $"E/W: {position.x:F1}\n" +
                               $"Up/Down: {position.y:F1}\n" +
                               $"N/S: {position.z:F1}\n" +
                               $"Facing: {rotation:F0}° ({cardinal})";

                UpdateDisplay(displayStr);
            }
            catch (System.Exception ex)
            {
                if (!(ex is System.NullReferenceException))
                {
                    LoggerInstance.Error($"Error in OnUpdate: {ex.Message}");
                }
            }
        }

        private void HandleUIInteractions()
        {
            if (displayPanel == null) return;

            var mousePos = UnityEngine.Input.mousePosition;
            var image = displayPanel.GetComponent<Image>();
            var isMouseOver = RectTransformUtility.RectangleContainsScreenPoint(displayRect, mousePos);

            // Handle hover effect
            if (image != null)
            {
                image.color = new Color(0, 0, 0, isMouseOver ? 0.8f : 0.5f);
            }

            // Handle dragging
            if (UnityEngine.Input.GetMouseButtonDown(0) && isMouseOver)
            {
                isDragging = true;
                dragStartPosition = displayRect.anchoredPosition;
                dragStartMousePosition = mousePos;
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                // Save position
                Config.GetEntry<float>("xPos").Value = displayRect.anchoredPosition.x;
                Config.GetEntry<float>("yPos").Value = displayRect.anchoredPosition.y;
                Config.SaveToFile(false);
            }

            if (isDragging)
            {
                Vector2 difference = (Vector2)mousePos - dragStartMousePosition;
                displayRect.anchoredPosition = dragStartPosition + difference;
            }
        }

        private string GetCardinalDirection(float angle)
        {
            string[] cardinals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
            return cardinals[(int)Mathf.Round(angle / 45f)];
        }

        private void UpdateDisplay(string text)
        {
            if (displayText != null)
            {
                displayText.text = text;
            }
        }

        public override void OnPreferencesSaved()
        {
            try
            {
                LoggerInstance.Msg("Preferences saved!");
                float xPos = Config.GetEntry<float>("xPos").Value;
                float yPos = Config.GetEntry<float>("yPos").Value;
                if (displayRect != null)
                {
                    displayRect.anchoredPosition = new Vector2(xPos, yPos);
                }
            }
            catch (System.Exception ex)
            {
                LoggerInstance.Error($"Error updating preferences: {ex.Message}");
            }
        }

        public static void SaveConfig()
        {
            Config?.SaveToFile(false);
        }
    }

    // Window drag handler component
    public class WindowDragHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private bool isDragging;
        private Vector2 dragStartPosition;
        private Vector2 dragStartMousePosition;

        public void Initialize(RectTransform rect)
        {
            rectTransform = rect;
            var eventTrigger = gameObject.AddComponent<EventTrigger>();

            // Add pointer down event
            var pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback = new EventTrigger.TriggerEvent();
            pointerDown.callback.AddListener((data) => OnPointerDown());
            eventTrigger.triggers.Add(pointerDown);

            // Add pointer up event
            var pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback = new EventTrigger.TriggerEvent();
            pointerUp.callback.AddListener((data) => OnPointerUp());
            eventTrigger.triggers.Add(pointerUp);

            // Add drag event
            var drag = new EventTrigger.Entry();
            drag.eventID = EventTriggerType.Drag;
            drag.callback = new EventTrigger.TriggerEvent();
            drag.callback.AddListener((data) => OnDrag((PointerEventData)data));
            eventTrigger.triggers.Add(drag);
        }

        private void OnPointerDown()
        {
            isDragging = true;
            dragStartPosition = rectTransform.anchoredPosition;
            dragStartMousePosition = UnityEngine.Input.mousePosition;
        }

        private void OnPointerUp()
        {
            if (isDragging)
            {
                isDragging = false;
                // Save the new position
                ModMain.xPos = rectTransform.anchoredPosition.x;
                ModMain.yPos = rectTransform.anchoredPosition.y;
                ModMain.Config.GetEntry<float>("xPos").Value = ModMain.xPos;
                ModMain.Config.GetEntry<float>("yPos").Value = ModMain.yPos;
                ModMain.SaveConfig();
            }
        }

        private void OnDrag(PointerEventData eventData)
        {
            if (isDragging && rectTransform != null)
            {
                Vector2 currentMousePosition = UnityEngine.Input.mousePosition;
                Vector2 difference = currentMousePosition - dragStartMousePosition;
                rectTransform.anchoredPosition = dragStartPosition + difference;
            }
        }
    }

    // Hover effect handler component
    public class HoverHandler : MonoBehaviour
    {
        private Image targetImage;
        private Color defaultColor;
        private Color hoverColor;

        public void Initialize(Image image, Color defaultCol, Color hoverCol)
        {
            targetImage = image;
            defaultColor = defaultCol;
            hoverColor = hoverCol;

            var eventTrigger = gameObject.AddComponent<EventTrigger>();

            // Add pointer enter event
            var pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback = new EventTrigger.TriggerEvent();
            pointerEnter.callback.AddListener((data) => OnPointerEnter());
            eventTrigger.triggers.Add(pointerEnter);

            // Add pointer exit event
            var pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback = new EventTrigger.TriggerEvent();
            pointerExit.callback.AddListener((data) => OnPointerExit());
            eventTrigger.triggers.Add(pointerExit);
        }

        private void OnPointerEnter()
        {
            if (targetImage != null)
            {
                targetImage.color = hoverColor;
            }
        }

        private void OnPointerExit()
        {
            if (targetImage != null)
            {
                targetImage.color = defaultColor;
            }
        }
    }
} 