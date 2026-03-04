using HarmonyLib;

namespace GorillaLibrary.Modding.Patches
{
    [HarmonyPatch(typeof(GorillaTagger), "Start")]
    internal static class PostInitializedPatch
    {
        public static void Postfix() => GameModeEvents.Instance.TriggerGameInitialized();
    }
}
