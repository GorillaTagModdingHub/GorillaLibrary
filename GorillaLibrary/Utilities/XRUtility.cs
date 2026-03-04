using System.Collections.Generic;
using UnityEngine;

namespace GorillaLibrary.Utilities;

public static class XRUtility
{
    public static bool IsXRSubsystemActive
    {
        get
        {
            if (!_isSubsystemActive.HasValue)
            {
                List<ISubsystem> xrDisplaySubsystems = [];
                SubsystemManager.GetSubsystems(xrDisplaySubsystems);
                _isSubsystemActive = xrDisplaySubsystems.Count > 0 && xrDisplaySubsystems.Exists(subystem => subystem.running);
            }

            return _isSubsystemActive.Value;
        }
    }

    private static bool? _isSubsystemActive = null;
}
