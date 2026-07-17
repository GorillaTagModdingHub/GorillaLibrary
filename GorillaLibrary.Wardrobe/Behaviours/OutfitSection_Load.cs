using System.Linq;
using static CosmeticWardrobe;
using static GorillaNetworking.CosmeticsController;

namespace GorillaLibrary.Wardrobe.Behaviours;

internal class OutfitSection_Load : WardrobeCategory
{
    public override string Title => "Load";

    public void Awake()
    {
        Events.Cosmetics.OnWornCosmeticsUpdated += UpdateCosmetics;
    }

    public override void ApplyCosmetic(CosmeticWardrobeSelection selection, int index)
    {
        var outfits = instance.savedOutfits;
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

    public override int GetSize()
    {
        return maxOutfits;
    }
}
