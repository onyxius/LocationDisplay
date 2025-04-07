using MelonLoader;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;

[assembly: MelonInfo(typeof(LocationDisplay.Core), "Location Display", "1.0.0", "Onyxi", null)]
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