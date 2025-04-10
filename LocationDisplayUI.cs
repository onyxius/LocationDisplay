using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Il2Cpp;
using MelonLoader;
using LocationDisplay.Hooks;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes;
using Exception = System.Exception;

namespace LocationDisplay
{
    /// <summary>
    /// UI component that displays the player's current location and time.
    /// This component is attached to a GameObject and handles the visual representation
    /// of the location and time information.
    /// </summary>
    public class LocationDisplayUI : MonoBehaviour
    {
        // Static instance for easy access
        public static LocationDisplayUI Instance { get; private set; }

        // UI Components
        private Text locationText;
        private Text timeText;
        private Image backgroundPanel;

        private void Awake()
        {
            Instance = this;
            InitializeUI();
        }

        private void InitializeUI()
        {
            try
            {
                // Add Canvas components
                var canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // Make sure it's on top

                var canvasScaler = gameObject.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.matchWidthOrHeight = 0.5f;

                gameObject.AddComponent<GraphicRaycaster>();

                // Create background panel
                backgroundPanel = gameObject.AddComponent<Image>();
                backgroundPanel.color = new Color(0f, 0f, 0f, 0.7f);

                // Create location text
                var locationObj = new GameObject("LocationText");
                locationObj.transform.SetParent(transform, false);
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
                timeObj.transform.SetParent(transform, false);
                timeText = timeObj.AddComponent<Text>();
                timeText.color = Color.white;
                timeText.fontSize = 14;
                timeText.alignment = TextAnchor.MiddleRight;
                timeText.rectTransform.anchorMin = new Vector2(0, 0);
                timeText.rectTransform.anchorMax = new Vector2(1, 1);
                timeText.rectTransform.offsetMin = new Vector2(10, 10);
                timeText.rectTransform.offsetMax = new Vector2(-10, -10);

                // Set up RectTransform for the window
                var rectTransform = GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(300, 100);
                rectTransform.anchoredPosition = Vector2.zero;

                // Add draggable component
                var draggable = gameObject.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Drag;
                entry.callback = new EventTrigger.TriggerEvent();
                entry.callback.AddListener((UnityAction<BaseEventData>)delegate(BaseEventData eventData) {
                    var pointerEventData = eventData as PointerEventData;
                    if (pointerEventData != null)
                    {
                        rectTransform.anchoredPosition += pointerEventData.delta;
                    }
                });
                draggable.triggers.Add(entry);

                MelonLogger.Msg("[LocationDisplay] UI initialized successfully");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error initializing UI: {ex}");
            }
        }

        /// <summary>
        /// Updates the display with the current player information.
        /// </summary>
        private void Update()
        {
            try
            {
                var (x, y, z, direction) = GameHooks.GetPlayerCoordinates();
                var zone = GameHooks.GetCurrentZone();
                var time = GameHooks.GetCurrentTime();

                if (locationText != null)
                {
                    locationText.text = $"Zone: {zone}\nX: {x:F2} (E/W)\nY: {y:F2} (N/S)\nZ: {z:F2} (U/D)";
                }
                if (timeText != null)
                {
                    timeText.text = $"Time: {time}\nDir: {direction:F0}°";
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[LocationDisplay] Error updating player info: {ex}");
            }
        }

        /// <summary>
        /// Cleans up the static instance when the component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
} 