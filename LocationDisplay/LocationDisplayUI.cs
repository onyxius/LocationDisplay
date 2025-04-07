using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;
using MelonLoader;
using LocationDisplay.Hooks;
using Il2CppScripts;
using Assembly_CSharp;

namespace LocationDisplay
{
    public class LocationDisplayUI : MonoBehaviour
    {
        private TextMeshProUGUI displayText;
        private Image background;
        private RectTransform rectTransform;

        private void Start()
        {
            CreateUIElements();
            UpdateDisplay();
        }

        private void CreateUIElements()
        {
            // Create canvas if it doesn't exist
            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameObject.AddComponent<CanvasScaler>();
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // Create background panel
            var panel = new GameObject("DisplayPanel");
            panel.transform.SetParent(transform, false);
            background = panel.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black

            // Create text display
            var textObj = new GameObject("DisplayText");
            textObj.transform.SetParent(panel.transform, false);
            displayText = textObj.AddComponent<TextMeshProUGUI>();
            displayText.fontSize = 16;
            displayText.color = Color.white;
            displayText.alignment = TextAlignmentOptions.Left;

            // Set up rect transforms
            rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(20, -20);
            rectTransform.sizeDelta = new Vector2(300, 50);

            var textRect = displayText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
        }

        private void UpdateDisplay()
        {
            if (displayText == null) return;

            try
            {
                var player = GameHooks.Instance.LocalPlayer;
                if (player != null)
                {
                    Vector3 pos = player.transform.position;
                    float heading = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;
                    displayText.text = $"X: {pos.x:F2}\nY: {pos.z:F2}\nZ: {pos.y:F2}\nFacing: {heading:F0}°";
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error updating display: {ex}");
            }
        }

        private void Update()
        {
            UpdateDisplay();
        }
    }
} 