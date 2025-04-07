using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Il2CppTMPro;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using UnityEditor;

[assembly: MelonInfo(typeof(LocationDisplay.ModMain), "Location Display", "1.0.0", "Onyxi")]
[assembly: MelonGame("Visionary Realms", "Pantheon")]
[assembly: MelonColor(34, 139, 34, 255)] // Forest Green RGB values with alpha

namespace LocationDisplay
{
    public class UIHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Image backgroundImage;
        private bool isDragging;
        private Vector2 dragStartPosition;
        private Vector2 dragStartMousePosition;
        private MelonPreferences_Category config;
        private string currentLocationText = "";

        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(System.IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern System.IntPtr SetClipboardData(uint uFormat, System.IntPtr data);

        [DllImport("kernel32.dll")]
        private static extern System.IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        private static extern System.IntPtr GlobalLock(System.IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(System.IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern System.IntPtr GlobalFree(System.IntPtr hMem);

        private const uint GMEM_MOVEABLE = 0x0002;
        private const uint CF_UNICODETEXT = 13;

        public void Initialize(RectTransform rect, Image image, MelonPreferences_Category cfg)
        {
            rectTransform = rect;
            backgroundImage = image;
            config = cfg;
            CreateCopyButton();
        }

        private void CreateCopyButton()
        {
            var buttonObj = new GameObject("CopyButton");
            buttonObj.transform.SetParent(transform, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 0);
            buttonRect.anchorMax = new Vector2(1, 0);
            buttonRect.pivot = new Vector2(0.5f, 0);
            buttonRect.sizeDelta = new Vector2(0, 25);
            buttonRect.anchoredPosition = new Vector2(0, -30);

            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var buttonText = new GameObject("ButtonText");
            buttonText.transform.SetParent(buttonObj.transform, false);
            
            var textRect = buttonText.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = buttonText.AddComponent<TextMeshProUGUI>();
            tmp.text = "Copy Location";
            tmp.fontSize = 12;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Convert the method to an Il2Cpp action
            var il2cppAction = DelegateSupport.ConvertDelegate<UnityAction>(new System.Action(CopyToClipboard));
            button.onClick.AddListener(il2cppAction);

            // Add hover effect using mouse events
            buttonObj.AddComponent<ButtonHoverHandler>().Initialize(buttonImage);
        }

        public void SetLocationText(string text)
        {
            currentLocationText = text;
        }

        private void CopyToClipboard()
        {
            if (string.IsNullOrEmpty(currentLocationText)) return;

            var ptr = System.IntPtr.Zero;
            try
            {
                OpenClipboard(System.IntPtr.Zero);
                EmptyClipboard();
                var bytes = System.Text.Encoding.Unicode.GetBytes(currentLocationText + "\0");
                ptr = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);
                var locked = GlobalLock(ptr);
                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, locked, bytes.Length);
                GlobalUnlock(ptr);
                SetClipboardData(CF_UNICODETEXT, ptr);
            }
            finally
            {
                CloseClipboard();
                if (ptr != System.IntPtr.Zero)
                {
                    GlobalFree(ptr);
                }
            }
        }

        private void Update()
        {
            if (rectTransform == null || backgroundImage == null) return;

            var mousePos = UnityEngine.Input.mousePosition;
            var isMouseOver = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos);

            // Handle hover effect
            backgroundImage.color = new Color(0, 0, 0, isMouseOver || isDragging ? 0.8f : 0.5f);

            // Handle dragging
            if (UnityEngine.Input.GetMouseButtonDown(0) && isMouseOver)
            {
                isDragging = true;
                dragStartPosition = rectTransform.anchoredPosition;
                dragStartMousePosition = mousePos;
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                // Save position
                config.GetEntry<float>("xPos").Value = rectTransform.anchoredPosition.x;
                config.GetEntry<float>("yPos").Value = rectTransform.anchoredPosition.y;
                config.SaveToFile(false);
            }

            if (isDragging)
            {
                Vector2 difference = (Vector2)mousePos - dragStartMousePosition;
                rectTransform.anchoredPosition = dragStartPosition + difference;
                
                // Prevent mouse input from reaching the game
                UnityEngine.Input.ResetInputAxes();
            }
        }
    }

    public class ButtonHoverHandler : MonoBehaviour
    {
        private Image buttonImage;
        private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private Color hoverColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        private Color pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);

        public void Initialize(Image image)
        {
            buttonImage = image;
            buttonImage.color = normalColor;
        }

        private void OnPointerEnter()
        {
            if (buttonImage != null)
                buttonImage.color = hoverColor;
        }

        private void OnPointerExit()
        {
            if (buttonImage != null)
                buttonImage.color = normalColor;
        }

        private void OnPointerDown()
        {
            if (buttonImage != null)
                buttonImage.color = pressedColor;
        }

        private void OnPointerUp()
        {
            if (buttonImage != null)
                buttonImage.color = normalColor;
        }
    }

    public class ModMain : MelonMod
    {
        private static HarmonyLib.Harmony harmony;
        public static MelonPreferences_Category Config { get; private set; }
        private static GameObject displayCanvas;
        private static GameObject displayPanel;
        private static TextMeshProUGUI timeText;
        private static TextMeshProUGUI locationText;
        private static RectTransform displayRect;
        private static UIHandler uiHandler;

        public override void OnInitializeMelon()
        {
            try
            {
                // Register custom types
                ClassInjector.RegisterTypeInIl2Cpp<UIHandler>();

                // Initialize preferences
                Config = MelonPreferences.CreateCategory("Location_Display");
                Config.CreateEntry("xPos", 10f);
                Config.CreateEntry("yPos", 10f);
                Config.CreateEntry("fontSize", 14);

                // Initialize Harmony
                harmony = new HarmonyLib.Harmony("com.onyxi.locationdisplay");
                harmony.PatchAll(typeof(ModMain).Assembly);

                // Clean up any existing UI
                CleanupUI();

                // Create UI
                CreateUI();

                LoggerInstance.Msg("Location Display mod initialized!");
            }
            catch (System.Exception ex)
            {
                LoggerInstance.Error($"Failed to initialize mod: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void CleanupUI()
        {
            var existingCanvas = GameObject.Find("LocationDisplayCanvas");
            if (existingCanvas != null)
            {
                GameObject.Destroy(existingCanvas);
            }
        }

        private void CreateUI()
        {
            // Create canvas
            displayCanvas = new GameObject("LocationDisplayCanvas");
            var canvas = displayCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            var scaler = displayCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            displayCanvas.AddComponent<GraphicRaycaster>();
            UnityEngine.Object.DontDestroyOnLoad(displayCanvas);

            // Create main panel
            displayPanel = new GameObject("LocationDisplayPanel");
            displayPanel.transform.SetParent(displayCanvas.transform, false);

            // Add background image
            var image = displayPanel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.5f);

            // Setup RectTransform
            displayRect = displayPanel.GetComponent<RectTransform>();
            displayRect.sizeDelta = new Vector2(300, 100);
            displayRect.anchorMin = Vector2.zero;
            displayRect.anchorMax = Vector2.zero;
            displayRect.pivot = Vector2.zero;

            // Load saved position
            float xPos = Config.GetEntry<float>("xPos").Value;
            float yPos = Config.GetEntry<float>("yPos").Value;
            displayRect.anchoredPosition = new Vector2(xPos, yPos);

            // Create time text (centered at top)
            var timeObj = new GameObject("TimeText");
            timeObj.transform.SetParent(displayPanel.transform, false);

            var timeRect = timeObj.AddComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0, 1);
            timeRect.anchorMax = new Vector2(1, 1);
            timeRect.pivot = new Vector2(0.5f, 1);
            timeRect.sizeDelta = new Vector2(0, 25);
            timeRect.anchoredPosition = new Vector2(0, -5);

            timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.fontSize = Config.GetEntry<int>("fontSize").Value;
            timeText.color = Color.white;
            timeText.alignment = TextAlignmentOptions.Center;
            timeText.enableWordWrapping = false;
            timeText.overflowMode = TextOverflowModes.Overflow;

            // Create location text
            var locationObj = new GameObject("LocationText");
            locationObj.transform.SetParent(displayPanel.transform, false);

            var locationRect = locationObj.AddComponent<RectTransform>();
            locationRect.anchorMin = new Vector2(0, 0);
            locationRect.anchorMax = new Vector2(1, 1);
            locationRect.offsetMin = new Vector2(5, 25);
            locationRect.offsetMax = new Vector2(-5, -30);

            locationText = locationObj.AddComponent<TextMeshProUGUI>();
            locationText.fontSize = Config.GetEntry<int>("fontSize").Value;
            locationText.color = Color.white;
            locationText.alignment = TextAlignmentOptions.Left;
            locationText.enableWordWrapping = false;
            locationText.overflowMode = TextOverflowModes.Overflow;

            // Add UI handler
            uiHandler = displayPanel.AddComponent<UIHandler>();
            uiHandler.Initialize(displayRect, image, Config);

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
                var player = GameObject.Find("LocalPlayer");
                if (player == null)
                {
                    UpdateDisplay("00:00", "Location: Unknown");
                    return;
                }

                var position = player.transform.position;
                var rotation = player.transform.eulerAngles.y;
                var cardinal = GetCardinalDirection(rotation);

                var gameTimeText = GameObject.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
                var timeStr = gameTimeText != null ? gameTimeText.text : "00:00";

                var locationStr = $"E/W: {position.x:F1}\n" +
                                $"Up/Down: {position.y:F1}\n" +
                                $"N/S: {position.z:F1}\n" +
                                $"Facing: {rotation:F0}° ({cardinal})";

                UpdateDisplay(timeStr, locationStr);
            }
            catch (System.Exception ex)
            {
                if (!(ex is System.NullReferenceException))
                {
                    LoggerInstance.Error($"Error in OnUpdate: {ex.Message}");
                }
            }
        }

        private string GetCardinalDirection(float angle)
        {
            string[] cardinals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };
            return cardinals[(int)Mathf.Round(angle / 45f)];
        }

        private void UpdateDisplay(string time, string location)
        {
            if (timeText != null)
            {
                timeText.text = time;
            }
            if (locationText != null)
            {
                locationText.text = location;
                if (uiHandler != null)
                {
                    uiHandler.SetLocationText(location);
                }
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
} 