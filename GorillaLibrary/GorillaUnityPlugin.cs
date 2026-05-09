using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;

namespace GorillaLibrary;

public class GorillaUnityPlugin : BaseUnityPlugin
{
    public bool Enabled
    {
        get => _enabled.GetValueOrDefault(true);
        set
        {
            if (_enabled.HasValue && _enabled.Value == value) return;
            _enabled = value;
            _stateEntry.Value = value;
            enabled = value;
        }
    }

    public bool IsStateSupported
    {
        get
        {
            if (!_stateSupported.HasValue)
            {
                List<string> instanceMethods = AccessTools.GetMethodNames(this);
                _stateSupported = instanceMethods.Contains("OnEnable") || instanceMethods.Contains("OnDisable");
            }

            return _stateSupported.Value;
        }
    }

    private bool? _enabled, _stateSupported;

    internal ConfigEntry<bool> _stateEntry;
}
