using GorillaLibrary.Attributes;
using GorillaLibrary.Extensions;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CosmeticWardrobe;

namespace GorillaLibrary.Behaviours;

public class WardrobeController : MonoBehaviour
{
    public static CosmeticWardrobe ReferenceWardrobe;

    public bool UseOverrides => _currentAttribute != null;

    private CosmeticWardrobe _cosmeticWardrobe;

    private CosmeticWardrobeSelection[] _selectionArray;

    private TMP_Text _outfitText;

    private CosmeticButton _nextOutfit, _previousOutfit;

    private List<ModdedWardrobeSectionAttribute> _categories;

    private ModdedWardrobeSectionAttribute _currentAttribute;

    private WardrobeCategory _currentCategory;

    private int _categoryIndex;

    private CosmeticWardrobeCategory template;

    private GameObject categoryButtonParent;

    private readonly List<GameObject> baseObjects = [];

    private readonly Dictionary<WardrobeCategory, Tuple<Sprite, Sprite, Sprite>> icons = [];

    internal void Awake()
    {
        Events.Core.OnGameInitialized.Subscribe(Initialize);
    }

    private async void Initialize()
    {
        _cosmeticWardrobe = GetComponent<CosmeticWardrobe>();
        _selectionArray = _cosmeticWardrobe.GetField<CosmeticWardrobeSelection[]>("cosmeticCollectionDisplays");

        _outfitText = _cosmeticWardrobe.GetField<TMP_Text>("outfitText");
        _nextOutfit = _cosmeticWardrobe.GetField<CosmeticButton>("nextOutfit");
        _previousOutfit = _cosmeticWardrobe.GetField<CosmeticButton>("previousOutfit");

        if (_outfitText.IsObjectNull() || _nextOutfit.IsObjectNull() || _previousOutfit.IsObjectNull()) return;

        var baseCategories = Melon<Mod>.Instance.wardrobeCategories;
        _categories = (baseCategories != null && baseCategories.Count > 0) ? [null, .. baseCategories] : [];

        OnOutfitTextUpdate();

        var cosmeticCategoryButtons = _cosmeticWardrobe.GetField<CosmeticWardrobeCategory[]>("cosmeticCategoryButtons");
        template = cosmeticCategoryButtons.FirstOrDefault(category =>
        {
            CosmeticCategoryButton button = category.button;
            return button.GetField<SpriteRenderer>("equippedIcon").IsObjectExistent() && button.GetField<SpriteRenderer>("equippedLeftIcon").IsObjectExistent() && button.GetField<SpriteRenderer>("equippedRightIcon").IsObjectExistent();
        });
        categoryButtonParent = template.button.transform.parent.gameObject;

        foreach (var category in cosmeticCategoryButtons)
        {
            var button = category.button;
            baseObjects.Add(button.GetField<SpriteRenderer>("equippedIcon")?.gameObject);
            baseObjects.Add(button.GetField<SpriteRenderer>("equippedLeftIcon")?.gameObject);
            baseObjects.Add(button.GetField<SpriteRenderer>("equippedRightIcon")?.gameObject);
            baseObjects.Add(button.myText?.gameObject);
            baseObjects.Add(button.myTmpText?.gameObject);
            baseObjects.Add(button.gameObject);
        }

        GameObject customButtonStorage = new("CustomButtonStorage");
        customButtonStorage.transform.SetParent(categoryButtonParent.transform.parent);
        customButtonStorage.transform.localPosition = new Vector3(0.158f, 0.2271f, 0.3409f);
        customButtonStorage.transform.eulerAngles = categoryButtonParent.transform.eulerAngles;
        customButtonStorage.transform.localScale = categoryButtonParent.transform.localScale;
        customButtonStorage.AddComponent<Canvas>();

        HorizontalLayoutGroup layoutGroup = customButtonStorage.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.reverseArrangement = true;

        List<CosmeticCategoryButton> buttons = [];

        foreach (var category in _categories)
        {
            if (category == null) continue;

            var objects = new List<GameObject>();
            category.objects = objects;

            var buttonsInCategory = new List<CosmeticCategoryButton>();
            category.buttons = buttonsInCategory;

            List<Tuple<GameObject, GameObject, CosmeticCategoryButton>> elements = [];

            for (int i = 0; i < category.categories.Count; i++)
            {
                if (i >= 5) break;

                var section = category.categories[i];

                GameObject buttonContainer = new("ButtonContainer");
                buttonContainer.transform.SetParent(customButtonStorage.transform);
                buttonContainer.transform.localPosition = Vector3.zero;
                buttonContainer.transform.localEulerAngles = Vector3.zero;
                buttonContainer.AddComponent<RectTransform>().sizeDelta = Vector2.one * 0.1f;

                GameObject buttonObject = Instantiate(template.button.gameObject);
                CosmeticCategoryButton newButton = buttonObject.GetComponent<CosmeticCategoryButton>();
                newButton.isOn = false;
                newButton.enabled = true;
                newButton.myText = null;
                newButton.myTmpText = null;
                newButton.UpdateColor();
                newButton.onPressed += OnCategorySelection;

                buttonObject.transform.SetParent(buttonContainer.transform);
                buttonObject.transform.localPosition = Vector3.zero;
                buttonObject.transform.localEulerAngles = Vector3.zero;

                GameObject newButtonText = Instantiate(template.button.myTmpText.gameObject);
                newButtonText.transform.SetParent(template.button.transform.parent);

                newButton.myTmpText = newButtonText.GetComponent<TMP_Text>();
                newButton.myTmpText.text = section.Title.ToUpper();

                objects.Add(buttonContainer);
                objects.Add(newButtonText);
                elements.Add(Tuple.Create(template.button.myTmpText.gameObject, newButtonText, newButton));

                string[] spriteNames = ["equippedIcon", "equippedLeftIcon", "equippedRightIcon"];

                for (int j = 0; j < spriteNames.Length; j++)
                {
                    SpriteRenderer originalSprite = template.button.GetField<SpriteRenderer>(spriteNames[j]);
                    GameObject spriteObject = Instantiate(originalSprite.gameObject);
                    spriteObject.transform.SetParent(template.button.transform.parent);
                    objects.Add(spriteObject);
                    elements.Add(Tuple.Create(originalSprite.gameObject, spriteObject, newButton));
                    newButton.SetField(spriteNames[j], spriteObject.GetComponent<SpriteRenderer>());
                }

                buttons.Add(newButton);
                buttonsInCategory.Add(newButton);
            }

            await Awaitable.EndOfFrameAsync();

            foreach (var tuple in elements)
            {
                Vector3 initialOffset = tuple.Item1.transform.position - template.button.transform.position;
                tuple.Item2.transform.position = initialOffset + tuple.Item3.transform.position;
                tuple.Item2.transform.rotation = tuple.Item1.transform.rotation;
                tuple.Item2.transform.localScale = tuple.Item1.transform.localScale;
                tuple.Item2.GetComponent<ReparentOnAwakeWithRenderer>()?.InvokeMethod("OnEnable");
            }

            objects.ForEach(gameObject => gameObject.SetActive(false));
        }

        Destroy(layoutGroup);

        await Awaitable.EndOfFrameAsync();

        foreach (var button in buttons)
        {
            button.SetField("startingPos", button.transform.localPosition);
            button.UpdateColor();
        }

        WardrobeCategory.UpdateCosmeticsRequest.Subscribe(UpdateCosmetics);
        WardrobeCategory.SetIconRequest.Subscribe(SetIcons);
    }

    internal void UpdateCosmetics(WardrobeCategory source)
    {
        Melon<Mod>.Logger.Msg("section");
        // if (section != source) return;
        OnCosmeticsUpdated();
    }

    internal void OnCosmeticsUpdated()
    {
        UpdateCosmeticDisplays();
    }

    internal void SetIcons(WardrobeCategory source, Tuple<Sprite, Sprite, Sprite> tuple)
    {
        if (icons.ContainsKey(source)) icons[source] = tuple;
        else icons.Add(source, tuple);

        UpdateCategoryButtons();
    }

    internal void OnSelectionNavigateNext()
    {
        _currentCategory.startingDisplayIndex++;
        int size = _currentCategory.GetSize();
        if (_currentCategory.startingDisplayIndex >= size) _currentCategory.startingDisplayIndex = 0;

        UpdateCosmeticDisplays();
    }

    internal void OnSelectionNavigatePrev()
    {
        _currentCategory.startingDisplayIndex--;

        if (_currentCategory.startingDisplayIndex < 0)
        {
            int size = _currentCategory.GetSize();
            if (size % _selectionArray.Length == 0)
            {
                _currentCategory.startingDisplayIndex = size - _selectionArray.Length;
            }
            else
            {
                _currentCategory.startingDisplayIndex = size / _selectionArray.Length;
                _currentCategory.startingDisplayIndex *= _selectionArray.Length;
            }
        }

        UpdateCosmeticDisplays();
    }

    internal void OnCosmeticSelection(GorillaPressableButton button)
    {
        try
        {
            for (int i = 0; i < _selectionArray.Length; i++)
            {
                if (_selectionArray[i].selectButton != button) continue;

                _currentCategory.SelectCosmetic(i);
                break;
            }

            UpdateCosmeticDisplays();
        }
        catch(Exception ex)
        {
            Melon<Mod>.Logger.Error(ex);
        }
    }

    internal void OnPageNavigateNext()
    {
        _categoryIndex++;
        if (_categoryIndex >= _categories.Count) _categoryIndex = 0;
        HandlePageNavigate();
    }

    internal void OnPageNavigatePrev()
    {
        _categoryIndex--;
        if (_categoryIndex < 0) _categoryIndex = _categories.Count - 1;
        HandlePageNavigate();
    }

    internal void OnCategorySelection(GorillaPressableButton baseButton, bool isLeftHand)
    {
        for (int i = 0; i < _currentAttribute.buttons.Count; i++)
        {
            var button = _currentAttribute.buttons[i];

            if (button == baseButton)
            {
                var previousSection = _currentAttribute.categories[_currentAttribute.buttons.IndexOf(_currentAttribute.selectedButton)];

                _currentAttribute.categories[i].OnActivated(_currentAttribute.selectedButton == button);
                _currentAttribute.selectedButton = button;

                _currentCategory = _currentAttribute.categories[i];

                if (previousSection != _currentAttribute.categories[i])
                {
                    ReferenceWardrobe = _cosmeticWardrobe;

                    for (int k = 0; k < _selectionArray.Length; k++)
                    {
                        CosmeticWardrobeSelection cosmeticWardrobeSelection = _selectionArray[k];
                        _currentCategory.ResetCosmetic(cosmeticWardrobeSelection);
                    }

                    ReferenceWardrobe = null;

                    UpdateCosmeticDisplays();
                }

                UpdateCategoryButtons();

                break;
            }
        }
    }

    internal void UpdateCategoryButtons()
    {
        for (int i = 0; i < _currentAttribute.buttons.Count; i++)
        {
            var button = _currentAttribute.buttons[i];
            var section = _currentAttribute.categories[i];
            Tuple<Sprite, Sprite, Sprite> tuple = icons.ContainsKey(section) ? icons[section] : Tuple.Create<Sprite, Sprite, Sprite>(null, null, null);

            if (tuple.Item1 != null) button.SetIcon(tuple.Item1);
            else if (tuple.Item2 != null || tuple.Item3 != null) button.SetDualIcon(tuple.Item2, tuple.Item3);
            else button.SetIcon(null);

            int categorySize = section.GetSize();
            button.enabled = categorySize > 0;
            button.isOn = _currentAttribute.selectedButton == button;
            button.UpdateColor();
        }
    }

    internal void HandlePageNavigate()
    {
        var lastAttribute = _currentAttribute;
        _currentAttribute = _categories[_categoryIndex];

        baseObjects.Where(gameObject => gameObject.IsObjectExistent()).ForEach(gameObject => gameObject.SetActive(_currentAttribute == null));

        try
        {
            if (_currentAttribute == null)
            {
                if (lastAttribute != null)
                {
                    lastAttribute.categories.ForEach(section => section.OnPageHide());
                    lastAttribute.objects.ForEach(gameObject => gameObject.SetActive(false));

                    ReferenceWardrobe = _cosmeticWardrobe;

                    for (int i = 0; i < _selectionArray.Length; i++)
                    {
                        CosmeticWardrobeSelection cosmeticWardrobeSelection = _selectionArray[i];
                        _currentCategory.ResetCosmetic(cosmeticWardrobeSelection);
                    }

                    ReferenceWardrobe = null;
                }

                _currentCategory = null;
                categoryButtonParent.SetActive(true);

                _selectionArray.ForEach(selection => selection.displayHead.InvokeMethod("_ClearCurrent"));
                _cosmeticWardrobe.InvokeMethod("UpdateCategoryButtons");
                _cosmeticWardrobe.InvokeMethod("UpdateCosmeticDisplays");
            }
            else
            {
                if (_currentAttribute != lastAttribute)
                {
                    _currentAttribute.selectedButton = _currentAttribute.buttons.FirstOrDefault(button => button.enabled) ?? _currentAttribute.buttons[0];
                    UpdateCategoryButtons();
                }
                
                _currentCategory = _currentAttribute.categories[_currentAttribute.buttons.IndexOf(_currentAttribute.selectedButton)];
                UpdateCosmeticDisplays();

                _currentAttribute.categories.ForEach(section => section.OnPageShow());
                _currentAttribute.objects.ForEach(gameObject => gameObject.SetActive(true));

                if (lastAttribute != null)
                {
                    lastAttribute.objects.ForEach(gameObject => gameObject.SetActive(false));

                    ReferenceWardrobe = _cosmeticWardrobe;

                    for (int i = 0; i < _selectionArray.Length; i++)
                    {
                        CosmeticWardrobeSelection cosmeticWardrobeSelection = _selectionArray[i];
                        _currentCategory.ResetCosmetic(cosmeticWardrobeSelection);
                    }

                    ReferenceWardrobe = null;
                }
            }
        }
        catch(Exception ex)
        {
            Melon<Mod>.Logger.Error(ex);
        }

        OnOutfitTextUpdate();
    }

    private void UpdateCosmeticDisplays()
    {
        if (_currentCategory == null) return;

        ReferenceWardrobe = _cosmeticWardrobe;

        for (int i = 0; i < _selectionArray.Length; i++)
        {
            CosmeticWardrobeSelection cosmeticWardrobeSelection = _selectionArray[i];
            _currentCategory.ApplyCosmetic(cosmeticWardrobeSelection, _currentCategory.startingDisplayIndex + i);
        }

        ReferenceWardrobe = null;

        int categorySize = _currentCategory.GetSize();

        GorillaPressableButton nextSelection = _cosmeticWardrobe.GetField<GorillaPressableButton>("nextSelection");
        nextSelection.enabled = categorySize > _selectionArray.Length;
        nextSelection.UpdateColor();

        GorillaPressableButton prevSelection = _cosmeticWardrobe.GetField<GorillaPressableButton>("prevSelection");
        prevSelection.enabled = categorySize > _selectionArray.Length;
        prevSelection.UpdateColor();
    }

    internal void OnOutfitTextUpdate()
    {
        if (_currentAttribute != null) _outfitText.text = _currentAttribute.Title.ToUpper();
        else _outfitText.text = "WARDROBE";

        _nextOutfit.enabled = true;
        _nextOutfit.UpdateColor();
        _previousOutfit.enabled = true;
        _previousOutfit.UpdateColor();
    }
}
