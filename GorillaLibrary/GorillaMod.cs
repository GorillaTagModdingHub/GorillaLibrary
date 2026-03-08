using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace GorillaLibrary;

public class GorillaMod : MelonMod
{
    public bool Enabled
    {
        get => _enabled.GetValueOrDefault(true);
        set
        {
            if (_enabled.HasValue && _enabled.Value == value) return;
            _enabled = value;
            _statePreference.Value = value;
            if (value) OnMelonEnabled();
            else OnMelonDisabled();
        }
    }

    public bool IsStateSupported
    {
        get
        {
            if (!_stateSupported.HasValue)
            {
                List<string> instanceMethods = AccessTools.GetMethodNames(this);
                _stateSupported = instanceMethods.Contains("OnMelonEnabled") || instanceMethods.Contains("OnMelonDisabled");
            }

            return _stateSupported.Value;
        }
    }

    public ReadOnlyCollection<MelonPreferences_Category> Categories => new(_categories);

    private readonly List<MelonPreferences_Category> _categories = [];

    private bool? _enabled, _stateSupported;

    internal MelonPreferences_Entry<bool> _statePreference;

    public MelonPreferences_Category CreateCategory(string identifier) => RegisterCategory(MelonPreferences.CreateCategory(identifier));

    public MelonPreferences_Category CreateCategory(string identifier, string displayName) => RegisterCategory(MelonPreferences.CreateCategory(identifier, displayName));

    public MelonPreferences_Category CreateCategory(string identifier, string displayName = null, bool hidden = false, bool save = true) => RegisterCategory(MelonPreferences.CreateCategory(identifier, displayName, hidden, save));

    private MelonPreferences_Category RegisterCategory(MelonPreferences_Category category)
    {
        category.SetFilePath(Path.Combine(MelonEnvironment.UserDataDirectory, $"{Info?.Name}.cfg"));
        _categories.Add(category);
        return category;
    }

    public virtual void OnMelonEnabled()
    {

    }

    public virtual void OnMelonDisabled()
    {

    }
}
