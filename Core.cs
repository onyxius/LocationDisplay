using MelonLoader;

[assembly: MelonInfo(typeof(LocationDisplay.Core), "LocationDisplay", "1.0.0", "onyxi", null)]
[assembly: MelonGame("Visionary Realms", "Pantheon")]

namespace LocationDisplay
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }
    }
}