using GorillaLibrary.Behaviours;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaLibrary.Attributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class WardrobeCategoryAttribute(string title, params Type[] types) : Attribute
{
    public string Title = title;

    public Type[] SectionTypes = types;

    internal List<WardrobeSection> sections;

    internal List<GameObject> objects;

    internal List<CosmeticCategoryButton> buttons;

    internal CosmeticCategoryButton selectedButton;
}