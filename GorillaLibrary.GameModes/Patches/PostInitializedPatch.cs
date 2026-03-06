using GorillaLibrary.GameModes.Events;
using HarmonyLib;

namespace GorillaLibrary.GameModes.Patches
{
    [HarmonyPatch(typeof(GorillaTagger), "Start")]
    internal static class PostInitializedPatch
    {
        public static void Postfix() => GameModeEvents.Instance.TriggerGameInitialized();
    }
}
