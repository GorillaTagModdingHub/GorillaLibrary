using HarmonyLib;

namespace GorillaLibrary.Patches
{
    [HarmonyPatch(typeof(GorillaTagger), "Start")]
    internal static class PostInitializedPatch
    {
        public static void Postfix()
        {
            Plugin.Instance.OnGameInitialized();
        }
    }
}
