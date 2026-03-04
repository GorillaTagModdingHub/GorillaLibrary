using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace GorillaLibrary.Utilities;

public static class RigUtility
{
    public static VRRig LocalRig => VRRig.LocalRig ?? GorillaTagger.Instance.offlineVRRig;

    public static bool Initialized => (bool)AccessTools.Property(typeof(VRRig).Assembly.GetType("VRRigCache"), "isInitialized").GetValue(null);

    public static Dictionary<NetPlayer, RigContainer> Rigs => (Dictionary<NetPlayer, RigContainer>)AccessTools.Field(typeof(VRRig).Assembly.GetType("VRRigCache"), "rigsInUse").GetValue(null);

    public static bool TryGetRig(NetPlayer player, out RigContainer rig)
    {
        Assembly assembly = typeof(VRRig).Assembly;
        var rigCache = assembly.GetType("VRRigCache");
        object instance = AccessTools.Property(rigCache, "Instance").GetValue(rigCache, null);

        if (instance == null)
        {
            rig = null;
            return false;
        }

        object[] parameters = [player, null];
        bool result = (bool)AccessTools.Method(rigCache, "TryGetVrrig", [typeof(NetPlayer), assembly.GetType("RigContainer&")]).Invoke(instance, parameters);

        rig = (RigContainer)parameters[1];
        return result;
    }

    public static RigContainer TryGetRig(NetPlayer player) => TryGetRig(player, out RigContainer playerRig) ? playerRig : null;
}