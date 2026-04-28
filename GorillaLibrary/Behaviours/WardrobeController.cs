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

    public bool UseCustomCategory => _currentCategory != null;

    private CosmeticWardrobe _cosmeticWardrobe;

    private CosmeticWardrobeSelection[] _selectionArray;

    private TMP_Text _outfitText;

    private CosmeticButton _nextOutfit, _previousOutfit;

    private WardrobeCategoryAttribute _currentCategory;

    private List<WardrobeCategoryAttribute> _categories;

    private WardrobeSection section;

    private int _categoryIndex;

    private CosmeticWardrobeCategory template;

    private GameObject categoryButtonParent;

    private readonly List<GameObject> baseObjects = [];

    private readonly Dictionary<WardrobeSection, Tuple<Sprite, Sprite, Sprite>> icons = [];

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

            for (int i = 0; i < category.sections.Count; i++)
            {
                if (i >= 5) break;

                var section = category.sections[i];

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

        WardrobeSection.UpdateCosmeticsRequest.Subscribe(UpdateCosmetics);
        WardrobeSection.SetIconRequest.Subscribe(SetIcons);
    }

    internal void UpdateCosmetics(WardrobeSection source)
    {
        Melon<Mod>.Logger.Msg("section");
        // if (section != source) return;
        OnCosmeticsUpdated();
    }

    internal void OnCosmeticsUpdated()
    {
        UpdateCosmeticDisplays();
    }

    internal void SetIcons(WardrobeSection source, Tuple<Sprite, Sprite, Sprite> tuple)
    {
        if (icons.ContainsKey(source)) icons[source] = tuple;
        else icons.Add(source, tuple);

        UpdateCategoryButtons();
    }

    internal void OnSelectionNavigateNext()
    {
        section.startingDisplayIndex++;
        int size = section.GetSectionSize();
        if (section.startingDisplayIndex >= size) section.startingDisplayIndex = 0;

        UpdateCosmeticDisplays();
    }

    internal void OnSelectionNavigatePrev()
    {
        section.startingDisplayIndex--;

        if (section.startingDisplayIndex < 0)
        {
            int size = section.GetSectionSize();
            if (size % _selectionArray.Length == 0)
            {
                section.startingDisplayIndex = size - _selectionArray.Length;
            }
            else
            {
                section.startingDisplayIndex = size / _selectionArray.Length;
                section.startingDisplayIndex *= _selectionArray.Length;
            }
        }

        UpdateCosmeticDisplays();
    }

    internal void OnCosmeticSelection(GorillaPressableButton button)
    {
        for (int i = 0; i < _selectionArray.Length; i++)
        {
            if (_selectionArray[i].selectButton != button) continue;

            section.SelectCosmetic(i);
            break;
        }

        UpdateCosmeticDisplays();
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
        for (int i = 0; i < _currentCategory.buttons.Count; i++)
        {
            var button = _currentCategory.buttons[i];

            if (button == baseButton)
            {
                var previousSection = _currentCategory.sections[_currentCategory.buttons.IndexOf(_currentCategory.selectedButton)];

                _currentCategory.sections[i].OnSectionActivated(_currentCategory.selectedButton == button);
                _currentCategory.selectedButton = button;

                section = _currentCategory.sections[i];

                if (previousSection != _currentCategory.sections[i])
                {
                    ReferenceWardrobe = _cosmeticWardrobe;

                    for (int k = 0; k < _selectionArray.Length; k++)
                    {
                        CosmeticWardrobeSelection cosmeticWardrobeSelection = _selectionArray[k];
                        section.ResetCosmetic(cosmeticWardrobeSelection);
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
        for (int i = 0; i < _currentCategory.buttons.Count; i++)
        {
            var button = _currentCategory.buttons[i];
            var section = _currentCategory.sections[i];
            Tuple<Sprite, Sprite, Sprite> tuple = icons.ContainsKey(section) ? icons[section] : Tuple.Create<Sprite, Sprite, Sprite>(null, null, null);

            if (tuple.Item1 != null) button.SetIcon(tuple.Item1);
            else if (tuple.Item2 != null || tuple.Item3 != null) button.SetDualIcon(tuple.Item2, tuple.Item3);
            else button.SetIcon(null);

            int categorySize = section.GetSectionSize();
            button.enabled = categorySize > 0;
            button.isOn = _currentCategory.selectedButton == button;
            button.UpdateColor();
        }
    }

    internal void HandlePageNavigate()
    {
        var previousCategory = _currentCategory;
        _currentCategory = _categories[_categoryIndex];

        if (_currentCategory != null)
        {
            categoryButtonParent.SetActive(false);

            if (previousCategory != _currentCategory)
            {
                UpdateCategoryButtons();
                _currentCategory.selectedButton = _currentCategory.buttons.FirstOrDefault(button => button.enabled) ?? _currentCategory.buttons[0];
                UpdateCategoryButtons();

                _currentCategory.sections.ForEach(section => section.OnCategoryShown());
                _currentCategory.objects.ForEach(gameObject => gameObject.SetActive(true));

                if (previousCategory != null)
                {
                    previousCategory.objects.ForEach(gameObject => gameObject.SetActive(false));

                    ReferenceWardrobe = _cosmeticWardrobe;

                    for (int i = 0; i < _selectionArray.Length; i++)
                    {
                        CosmeticWardrobeSelection cosmeticWardrobeSelection = _selectionArray[i];
                        section.ResetCosmetic(cosmeticWardrobeSelection);
                    }

                    ReferenceWardrobe = null;
                }

                section = _currentCategory.sections[_currentCategory.buttons.IndexOf(_currentCategory.selectedButton)];
                UpdateCosmeticDisplays();
            }
        }
        else
        {
            categoryButtonParent.SetActive(true);

            _selectionArray.ForEach(selection => selection.displayHead.InvokeMethod("_ClearCurrent"));
            _cosmeticWardrobe.InvokeMethod("UpdateCategoryButtons");
            _cosmeticWardrobe.InvokeMethod("UpdateCosmeticDisplays");

            if (previousCategory != null)
            {
                previousCategory.sections.ForEach(section => section.OnCategoryHidden());
                previousCategory.objects.ForEach(gameObject => gameObject.SetActive(false));

                ReferenceWardrobe = _cosmeticWardrobe;

                for (int i = 0; i < _selectionArray.Length; i++)
                {
                    CosmeticWardrobeSelection cosmeticWardrobeSelection = _selectionArray[i];
                    section.ResetCosmetic(cosmeticWardrobeSelection);
                }

                ReferenceWardrobe = null;
            }

            section = null;
        }

        baseObjects.Where(gameObject => gameObject.IsObjectExistent()).ForEach(gameObject => gameObject.SetActive(_currentCategory == null));

        OnOutfitTextUpdate();
    }

    private void UpdateCosmeticDisplays()
    {
        if (section == null) return;

        ReferenceWardrobe = _cosmeticWardrobe;

        for (int i = 0; i < _selectionArray.Length; i++)
        {
            CosmeticWardrobeSelection cosmeticWardrobeSelection = _selectionArray[i];
            section.ApplyCosmetic(cosmeticWardrobeSelection, section.startingDisplayIndex + i);
        }

        ReferenceWardrobe = null;

        int categorySize = section.GetSectionSize();

        GorillaPressableButton nextSelection = _cosmeticWardrobe.GetField<GorillaPressableButton>("nextSelection");
        nextSelection.enabled = categorySize > _selectionArray.Length;
        nextSelection.UpdateColor();

        GorillaPressableButton prevSelection = _cosmeticWardrobe.GetField<GorillaPressableButton>("prevSelection");
        prevSelection.enabled = categorySize > _selectionArray.Length;
        prevSelection.UpdateColor();
    }

    internal void OnOutfitTextUpdate()
    {
        try
        {
            _outfitText.text = (UseCustomCategory ? _currentCategory?.Title : "Base Wardrobe")?.ToUpper();
        }
        catch
        {

        }
        _nextOutfit.enabled = true;
        _nextOutfit.UpdateColor();
        _previousOutfit.enabled = true;
        _previousOutfit.UpdateColor();
    }
}
