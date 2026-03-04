using GorillaTag;
using UnityEngine;

namespace GorillaLibrary.Events;

public class RigEvents
{
    public static DelegateListProcessor<VRRig, NetPlayer> OnRigAdded;

    public static DelegateListProcessor<VRRig> OnRigRemoved;

    public static DelegateListProcessor<VRRig, Color> OnColourChanged;
}
