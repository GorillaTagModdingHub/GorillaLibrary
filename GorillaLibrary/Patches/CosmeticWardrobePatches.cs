using GorillaLibrary.Behaviours;
using HarmonyLib;

namespace GorillaLibrary.Patches;

[HarmonyPatch(typeof(CosmeticWardrobe))]
internal class CosmeticWardrobePatches
{
    [HarmonyPatch("Start"), HarmonyPostfix, HarmonyWrapSafe]
    public static void StartPatch(CosmeticWardrobe __instance)
    {
        __instance.AddComponent<WardrobeController>();
    }

    [HarmonyPatch("HandleCosmeticsUpdated"), HarmonyPostfix, HarmonyWrapSafe]
    public static void HandleCosmeticsUpdatedPatch(CosmeticWardrobe __instance)
    {
        if (__instance.TryGetComponent(out WardrobeController controller) && controller.UseCustomCategory)
            controller.OnCosmeticsUpdated();
    }

    [HarmonyPatch("HandlePressedNextSelection"), HarmonyPrefix, HarmonyWrapSafe]
    public static bool HandleNextSelectionPatch(CosmeticWardrobe __instance)
    {
        if (__instance.TryGetComponent(out WardrobeController controller) && controller.UseCustomCategory)
        {
            controller.OnSelectionNavigateNext();
            return false;
        }

        return true;
    }

    [HarmonyPatch("HandlePressedPrevSelection"), HarmonyPrefix, HarmonyWrapSafe]
    public static bool HandlePrevSelectionPatch(CosmeticWardrobe __instance)
    {
        if (__instance.TryGetComponent(out WardrobeController controller) && controller.UseCustomCategory)
        {
            controller.OnSelectionNavigatePrev();
            return false;
        }

        return true;
    }

    [HarmonyPatch("HandlePressedSelectCosmeticButton"), HarmonyPrefix, HarmonyWrapSafe]
    public static bool HandleSelectCosmeticPatch(CosmeticWardrobe __instance, GorillaPressableButton button)
    {
        if (__instance.TryGetComponent(out WardrobeController controller) && controller.UseCustomCategory)
        {
            controller.OnCosmeticSelection(button);
            return false;
        }

        return true;
    }

    [HarmonyPatch("HandleChangeCategory"), HarmonyPrefix]
    public static bool ChangeCategoryPatch(CosmeticWardrobe __instance, GorillaPressableButton button, bool isLeft)
    {
        if (__instance.TryGetComponent(out WardrobeController controller) && controller.UseCustomCategory)
        {
            controller.OnCategorySelection(button, isLeft);
            return false;
        }

        return true;
    }

    [HarmonyPatch("HandlePressedNextOutfitButton"), HarmonyPrefix]
    public static bool HandleNextOutfitPatch(CosmeticWardrobe __instance)
    {
        if (__instance.TryGetComponent(out WardrobeController controller))
        {
            controller.OnPageNavigateNext();
            return false;
        }

        return false;
    }

    [HarmonyPatch("HandlePressedPrevOutfitButton"), HarmonyPrefix]
    public static bool HandlePrevOutfitPatch(CosmeticWardrobe __instance)
    {
        if (__instance.TryGetComponent(out WardrobeController controller))
        {
            controller.OnPageNavigatePrev();
            return false;
        }

        return false;
    }

    [HarmonyPatch("UpdateOutfitButtons"), HarmonyPrefix]
    public static bool OutfitTextUpdatePatch(CosmeticWardrobe __instance)
    {
        if (__instance.TryGetComponent(out WardrobeController controller))
        {
            controller.OnOutfitTextUpdate();
            return false;
        }

        return true;
    }
}
