using MelonLoader;
using UnityEngine;
using HarmonyLib;

namespace LocationDisplay;

public class ModMain : MelonMod
{
    public const string ModVersion = "1.0.0";
    private static GameObject displayObject;
    private static LocationDisplayUI locationDisplay;
    private static Harmony harmony;

    public override void OnInitializeMelon()
    {
        // Create mod preferences
        var modCategory = MelonPreferences.CreateCategory(nameof(LocationDisplay));
        var xPosition = modCategory.CreateEntry("xPosition", 20);
        var yPosition = modCategory.CreateEntry("yPosition", 20);
        var fontSize = modCategory.CreateEntry("fontSize", 16);
        var textColor = modCategory.CreateEntry("textColor", "#FFFFFF");
        var backgroundColor = modCategory.CreateEntry("backgroundColor", "#00000080");
        
        modCategory.SaveToFile(false);

        // Initialize Harmony patches
        harmony = new Harmony("com.onyxi.locationdisplay");
        harmony.PatchAll();

        // Initialize the display
        InitializeDisplay();
    }

    private void InitializeDisplay()
    {
        // Create a new GameObject to hold our UI
        displayObject = new GameObject("LocationDisplay");
        Object.DontDestroyOnLoad(displayObject);
        
        // Add our UI component
        locationDisplay = displayObject.AddComponent<LocationDisplayUI>();
    }

    public override void OnDeinitializeMelon()
    {
        // Clean up Harmony patches
        harmony?.UnpatchAll();
        
        // Clean up UI
        if (displayObject != null)
        {
            Object.Destroy(displayObject);
        }
    }
} 