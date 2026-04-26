using GorillaLibrary.Extensions;
using GorillaNetworking;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using static CosmeticWardrobe;
using static GorillaNetworking.CosmeticsController;

namespace GorillaLibrary.Behaviours;

internal class OutfitSection_Clone : WardrobeSection
{
    public override string Title => "Clone";

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
        var outfits = instance.GetField<CosmeticSet[]>("savedOutfits");
        outfits[index].CopyItems(instance.currentWornSet);

        var colours = instance.GetField<Vector3[]>("savedColors");
        colours[index] = new Vector3(VRRig.LocalRig.playerColor.r, VRRig.LocalRig.playerColor.g, VRRig.LocalRig.playerColor.b);

        instance.SetField("selectedOutfit", SelectedOutfit);
        instance.LoadSavedOutfit(index);

        UpdateCosmetics();
    }

    public override int GetSectionSize()
    {
        return (int)AccessTools.Field(typeof(CosmeticsController), "maxOutfits").GetValue(null);
    }

    public override void OnSectionActivated(bool hasActivated)
    {
        if (hasActivated)
        {
            int size = GetSectionSize();

            for (int i = 0; i < size; i++)
            {
                var outfits = instance.GetField<CosmeticSet[]>("savedOutfits");
                outfits[i].CopyItems(instance.currentWornSet);

                var colours = instance.GetField<Vector3[]>("savedColors");
                colours[i] = new Vector3(VRRig.LocalRig.playerColor.r, VRRig.LocalRig.playerColor.g, VRRig.LocalRig.playerColor.b);
            }

            instance.SetField("selectedOutfit", SelectedOutfit == 0 ? 1 : 0);
            instance.LoadSavedOutfit(0);

            UpdateCosmetics();
        }
    }
}
