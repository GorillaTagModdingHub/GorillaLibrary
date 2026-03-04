using GorillaLibrary.Models;
using HarmonyLib;
using UnityEngine;
using static GorillaNetworking.CosmeticsController;

namespace GorillaLibrary.Extensions;

public static class RigExtensions
{
    public static float GetScaleFactor(this VRRig rig)
    {
        return (float)AccessTools.Field(typeof(VRRig), "lastScaleFactor").GetValue(rig);
    }

    public static void GetCosmetics(this VRRig rig, out CosmeticSet currentWornSet, out CosmeticSet tryOnSet)
    {
        currentWornSet = rig.cosmeticSet;
        tryOnSet = rig.tryOnSet;
    }

    public static bool InTryOnRoom(this VRRig rig)
    {
        return rig.inTryOnRoom;
    }

    public static CosmeticSet GetCosmetics(this VRRig rig)
    {
        rig.GetCosmetics(out CosmeticSet current, out CosmeticSet tryOn);
        return rig.InTryOnRoom() ? tryOn : current;
    }

    public static GorillaIK GetGorilaIK(this VRRig rig)
    {
        GorillaIK ik = (GorillaIK)AccessTools.Field(typeof(VRRig), "myIk").GetValue(rig);
        return ik ?? rig.GetComponent<GorillaIK>();
    }

    public static Transform GetBone(this VRRig rig, GorillaRigBone bone)
    {
        GorillaIK ik = rig.GetGorilaIK();
        return bone switch
        {
            GorillaRigBone.Head => ik.headBone ?? rig.headMesh.transform,
            GorillaRigBone.Body => ik.bodyBone.Find("body"),
            GorillaRigBone.LeftUpperArm => ik.leftUpperArm ?? ik.bodyBone.Find("shoulder.L"),
            GorillaRigBone.RightUpperArm => ik.rightUpperArm ?? ik.bodyBone.Find("shoulder.R"),
            GorillaRigBone.LeftLowerArm => ik.leftLowerArm ?? ik.bodyBone.Find("shoulder.L/forearm.L"),
            GorillaRigBone.RightLowerArm => ik.rightLowerArm ?? ik.bodyBone.Find("shoulder.R/forearm.R"),
            GorillaRigBone.LeftHand => ik.leftHand,
            GorillaRigBone.RightHand => ik.rightHand,
            _ => null
        };
    }
}
