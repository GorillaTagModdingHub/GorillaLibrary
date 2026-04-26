using MelonLoader;
using System;
using UnityEngine;
using static CosmeticWardrobe;

namespace GorillaLibrary.Behaviours;

internal abstract class WardrobeSection : MonoBehaviour
{
    public abstract string Title { get; }

    internal static MelonEvent<WardrobeSection, Tuple<Sprite, Sprite, Sprite>> SetIconRequest;

    internal static MelonEvent<WardrobeSection> UpdateCosmeticsRequest;

    internal int startingDisplayIndex;

    public virtual void OnCategoryShown()
    {

    }

    public virtual void OnCategoryHidden()
    {

    }

    public abstract int GetSectionSize();

    public abstract void OnSectionActivated(bool hasActivated);

    public abstract void ApplyCosmetic(CosmeticWardrobeSelection selection, int index);

    public virtual void ResetCosmetic(CosmeticWardrobeSelection selection)
    {

    }

    public abstract void SelectCosmetic(int index);

    public void UpdateCosmetics()
    {
        UpdateCosmeticsRequest?.Invoke(this);
    }

    public void ClearIcons() => SetIcon(null);

    public void SetIcon(Sprite texture)
    {
        SetIconRequest?.Invoke(this, Tuple.Create<Sprite, Sprite, Sprite>(texture, null, null));
    }

    public void SetDualIcon(Sprite leftTexture, Sprite rightTexture)
    {
        SetIconRequest?.Invoke(this, Tuple.Create<Sprite, Sprite, Sprite>(null, leftTexture, rightTexture));
    }
}
