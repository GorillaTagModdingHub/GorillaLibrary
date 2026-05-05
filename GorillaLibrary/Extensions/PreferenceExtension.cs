using MelonLoader;
using MelonLoader.Preferences;
using System;

namespace GorillaLibrary.Extensions;

public static class PreferenceExtension
{
    [Obsolete]
    public static MelonPreferences_Entry<T> CreateSimpleEntry<T>(this MelonPreferences_Category category, string title, T defaultValue, string description)
    {
        return category.CreateSimpleEntry(title, defaultValue, description, null);
    }

    [Obsolete]
    public static MelonPreferences_Entry<T> CreateSimpleEntry<T>(this MelonPreferences_Category category, string title, T defaultValue, string description, ValueValidator validator)
    {
        return category.CreateEntry(title, defaultValue, title, description, false, false, validator);
    }

    public static MelonPreferences_Entry<T> AddEntry<T>(this MelonPreferences_Category category, string title, T defaultValue, string description)
    {
        return category.AddEntry(title, defaultValue, description, null);
    }

    public static MelonPreferences_Entry<T> AddEntry<T>(this MelonPreferences_Category category, string title, T defaultValue, string description, ValueValidator validator)
    {
        return category.CreateEntry(title, defaultValue, title, description, false, false, validator);
    }
}
