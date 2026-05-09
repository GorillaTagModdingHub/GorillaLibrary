using HarmonyLib;

namespace GorillaLibrary.Patches
{
    [HarmonyPatch(typeof(GorillaTagger), "Start")]
    internal static class PostInitializedPatch
    {
        public static void Postfix()
        {
            Events.Core.OnGameInitialized.Invoke();
            Events.Core.OnGameInitialized = null;
        }
    }
}
