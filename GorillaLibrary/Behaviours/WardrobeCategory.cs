using MelonLoader;
using System;
using UnityEngine;
using static CosmeticWardrobe;

namespace GorillaLibrary.Behaviours;

public abstract class WardrobeCategory : MonoBehaviour
{
    public abstract string Title { get; }

    internal static MelonEvent<WardrobeCategory, Tuple<Sprite, Sprite, Sprite>> SetIconRequest = new();

    internal static MelonEvent<WardrobeCategory> UpdateCosmeticsRequest = new();

    internal int startingDisplayIndex;

    public virtual void OnPageShow()
    {

    }

    public virtual void OnPageHide()
    {

    }

    public abstract int GetSize();

    public abstract void OnActivated(bool hasActivated);

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
