using MelonLoader;
using MelonLoader.Preferences;

namespace GorillaLibrary.Extensions;

public static class PreferenceExtension
{
    public static MelonPreferences_Entry<T> CreateSimpleEntry<T>(this MelonPreferences_Category category, string title, T defaultValue, string description)
    {
        return category.CreateSimpleEntry(title, defaultValue, description, null);
    }

    public static MelonPreferences_Entry<T> CreateSimpleEntry<T>(this MelonPreferences_Category category, string title, T defaultValue, string description, ValueValidator validator)
    {
        return category.CreateEntry(title, defaultValue, title, description, false, false, validator);
    }
}
