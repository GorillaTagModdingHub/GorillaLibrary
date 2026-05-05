using GorillaLibrary.Behaviours;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaLibrary.Attributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class ModdedWardrobeSectionAttribute(string title, params Type[] types) : Attribute
{
    public string Title = title;

    public Type[] SectionTypes = types;

    internal List<WardrobeCategory> categories;

    internal List<GameObject> objects;

    internal List<CosmeticCategoryButton> buttons;

    internal CosmeticCategoryButton selectedButton;
}