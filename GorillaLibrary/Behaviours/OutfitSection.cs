using GorillaLibrary.Extensions;
using GorillaNetworking;
using HarmonyLib;
using System.Linq;
using static CosmeticWardrobe;
using static GorillaNetworking.CosmeticsController;

namespace GorillaLibrary.Behaviours;

internal class OutfitSection : WardrobeSection
{
    public override string Title => "Load";

    public void Awake()
    {
        Events.Cosmetics.OnWornCosmeticsUpdated.Subscribe(UpdateCosmetics);
    }

    public override void ApplyCosmetic(CosmeticWardrobeSelection selection, int index)
    {
        var outfits = instance.GetField<CosmeticSet[]>("savedOutfits");
        var outfit = index != SelectedOutfit ? outfits[index] : instance.currentWornSet;

        selection.displayHead.SetCosmeticActiveArray([.. outfit.items.Select(item => item.displayName)], outfit.ToOnRightSideArray());
        selection.selectButton.enabled = true;
        selection.selectButton.isOn = index == SelectedOutfit;
        selection.selectButton.UpdateColor();
    }

    public override void SelectCosmetic(int index)
    {
        instance.LoadSavedOutfit(index);
    }

    public override int GetSectionSize()
    {
        return (int)AccessTools.Field(typeof(CosmeticsController), "maxOutfits").GetValue(null);
    }

    public override void OnSectionActivated(bool hasActivated)
    {
        if (hasActivated)
        {
            instance.InvokeMethod("ClearOutfits");
            UpdateCosmetics();
        }
    }
}
