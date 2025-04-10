using UnityEngine;
using UnityEngine.UI;
using System;
using MelonLoader;

namespace LocationDisplay
{
    public class ButtonClickHandler : MonoBehaviour
    {
        public Action OnClick;

        void Start()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(new Action(() => {
                    if (OnClick != null)
                        OnClick();
                }));
            }
        }

        public void HandleClick()
        {
            try
            {
                OnClick?.Invoke();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error in button click handler: {ex.Message}");
            }
        }
    }
} 