using GorillaLibrary.Events;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaLibrary.Patches;

[HarmonyPatch(typeof(RigContainer)), HarmonyWrapSafe, HarmonyPriority(Priority.First)]
internal class RigContainerPatches
{
    private static readonly Dictionary<RigContainer, RigEventSource> _dictionary = [];

    [HarmonyPatch(nameof(RigContainer.Creator), MethodType.Setter), HarmonyPostfix]
    public static void RigAddedPatch(RigContainer __instance, NetPlayer value)
    {
        if (_dictionary.ContainsKey(__instance)) return;

        var eventSource = new RigEventSource(__instance.Rig);
        __instance.Rig.OnColorChanged += eventSource.OnColourUpdated;
        _dictionary.Add(__instance, eventSource);

        RigEvents.OnRigAdded?.InvokeSafe(__instance.Rig, value);
    }

    [HarmonyPatch("OnDisable"), HarmonyPostfix]
    public static void RigRemovedPatch(RigContainer __instance)
    {
        if (_dictionary.TryGetValue(__instance, out var eventSource))
        {
            __instance.Rig.OnColorChanged -= eventSource.OnColourUpdated;
            _dictionary.Remove(__instance);

            RigEvents.OnRigRemoved?.InvokeSafe(__instance.Rig);
        }
    }

    private class RigEventSource(VRRig rig)
    {
        private readonly VRRig _rig = rig;

        public void OnColourUpdated(Color colour)
        {
            RigEvents.OnColourChanged?.InvokeSafe(_rig, colour);
        }
    }
}
