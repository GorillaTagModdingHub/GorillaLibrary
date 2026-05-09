using GorillaNetworking;
using HarmonyLib;

namespace GorillaLibrary.Patches;

[HarmonyPatch(typeof(CosmeticsController))]
internal class CosmeticControllerPatch
{
    [HarmonyPatch("UpdateWornCosmetics", [typeof(bool), typeof(bool)]), HarmonyPostfix]
    public static void WornCosmeticsUpdatePatch()
    {
        Events.Cosmetics.OnWornCosmeticsUpdated?.Invoke();
    }
}
