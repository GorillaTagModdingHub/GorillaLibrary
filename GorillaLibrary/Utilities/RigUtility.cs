using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GorillaLibrary.Utilities;

public static class RigUtility
{
    public static VRRig LocalRig => VRRig.LocalRig ?? GorillaTagger.Instance.offlineVRRig;

    public static bool Initialized => VRRigCache.isInitialized;

    public static Dictionary<NetPlayer, RigContainer> Rigs => (Dictionary<NetPlayer, RigContainer>)AccessTools.Field(typeof(VRRigCache), "rigsInUse").GetValue(null);

    public static NetInputTracker<float> LeftGrip { get; private set; }
    public static NetInputTracker<float> RightGrip { get; private set; }

    public static NetInputTracker<float> LeftTrigger { get; private set; }
    public static NetInputTracker<float> RightTrigger { get; private set; }

    public static NetInputTracker<bool> LeftFaceButton { get; private set; }
    public static NetInputTracker<bool> RightFaceButton { get; private set; }

    public static NetInputTracker<bool> LeftFaceButtonTouch { get; private set; }
    public static NetInputTracker<bool> RightFaceButtonTouch { get; private set; }

    public static void Initialize()
    {
        LeftGrip = new(NetInputType.Grip, true);
        RightGrip = new(NetInputType.Grip, false);

        LeftTrigger = new(NetInputType.Trigger, true);
        RightTrigger = new(NetInputType.Trigger, false);

        LeftFaceButton = new(NetInputType.FaceButtonPress, true);
        RightFaceButton = new(NetInputType.FaceButtonPress, false);

        LeftFaceButtonTouch = new(NetInputType.FaceButtonTouch, true);
        RightFaceButtonTouch = new(NetInputType.FaceButtonTouch, false);
    }

    public static bool TryGetRig(NetPlayer player, out RigContainer rig)
    {
        if (VRRigCache.Instance is not VRRigCache instance)
        {
            rig = null;
            return false;
        }

        Assembly assembly = typeof(VRRig).Assembly;
        object[] parameters = [player, null];
        bool result = (bool)AccessTools.Method(typeof(VRRigCache), "TryGetVrrig", [typeof(NetPlayer), assembly.GetType("RigContainer&")]).Invoke(instance, parameters);

        rig = (RigContainer)parameters[1];
        return result;
    }

    public static RigContainer GetRig(NetPlayer player) => TryGetRig(player, out RigContainer playerRig) ? playerRig : null;

    public class NetInputTracker<T>
    {
        private readonly NetInputType _inputType;
        private readonly bool _useLeftHand;

        public NetInputTracker(NetInputType type, bool leftHand)
        {
            if (typeof(T) != typeof(bool) && typeof(T) != typeof(float))
                throw new ArgumentException("NetInputTracker only accepts bool and float.");
            if (typeof(T) == typeof(float) && type != NetInputType.Grip && type != NetInputType.Trigger)
                throw new ArgumentException("Only trigger and grip inputs can be floats.");
            _inputType = type;
            _useLeftHand = leftHand;
        }

        //TODO: truncate garbage decimal data caused by packing
        public T GetValue(VRRig rig)
        {
            object returnValue = null; // need to convert to object first for type safety.

            switch (_inputType)
            {
                // Can't properly implement until garbage decimal is truncated
                case NetInputType.FaceButtonTouch:
                    returnValue = (_useLeftHand ? rig.leftThumb : rig.rightThumb).calcT > 0.5f;
                    return (T)returnValue;
                case NetInputType.FaceButtonPress:
                    returnValue = (_useLeftHand ? rig.leftThumb : rig.rightThumb).calcT > 0.5f;
                    return (T)returnValue;
                case NetInputType.Grip:
                    if (typeof(T) == typeof(bool))
                        returnValue = (_useLeftHand ? rig.leftMiddle : rig.rightMiddle).calcT > 0.5f;
                    else if (typeof(T) == typeof(float))
                        returnValue = (_useLeftHand ? rig.leftMiddle : rig.rightMiddle).calcT;
                    return (T)returnValue;
                case NetInputType.Trigger:
                    if (typeof(T) == typeof(bool))
                        returnValue = (_useLeftHand ? rig.leftIndex : rig.rightIndex).calcT > 0.5f;
                    else if (typeof(T) == typeof(float))
                        returnValue = (_useLeftHand ? rig.leftIndex : rig.rightIndex).calcT;
                    return (T)returnValue;
            }
            throw new Exception($"invalid input type {_inputType}.");
        }
    }

    public enum NetInputType
    {
        FaceButtonTouch,
        FaceButtonPress,
        Grip,
        Trigger
    }
}