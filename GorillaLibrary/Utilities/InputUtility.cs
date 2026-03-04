using GorillaLibrary.Models;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace GorillaLibrary.Utilities;

// https://github.com/developer9998/Bark/blob/76e893492845b620c6567be68f6e14bf0cbd49ba/Interaction/GestureTracker.cs#L15
public static class InputUtility
{
    public static InputDevice LeftController { get; private set; }
    public static InputDevice RightController { get; private set; }

    public static InputTracker<float> LeftGrip { get; private set; }
    public static InputTracker<float> RightGrip { get; private set; }
    public static InputTracker<float> LeftTrigger { get; private set; }
    public static InputTracker<float> RightTrigger { get; private set; }

    public static InputTracker<bool> LeftStickClick { get; private set; }
    public static InputTracker<bool> RightStickClick { get; private set; }
    public static InputTracker<bool> LeftPrimary { get; private set; }
    public static InputTracker<bool> RightPrimary { get; private set; }
    public static InputTracker<bool> LeftSecondary { get; private set; }
    public static InputTracker<bool> RightSecondary { get; private set; }

    public static InputTracker<Vector2> LeftStickAxis { get; private set; }
    public static InputTracker<Vector2> RightStickAxis { get; private set; }

    private static List<InputTracker> _inputTrackers;

    internal static void Initialize()
    {
        LeftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        RightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        var poller = Traverse.Create(ControllerInputPoller.instance);
        var pollerExt = Traverse.Create(new ControllerInputPollerExt());

        LeftGrip = new InputTracker<float>(poller.Field("leftControllerGripFloat"), XRNode.LeftHand);
        RightGrip = new InputTracker<float>(poller.Field("rightControllerGripFloat"), XRNode.RightHand);

        LeftTrigger = new InputTracker<float>(poller.Field("leftControllerIndexFloat"), XRNode.LeftHand);
        RightTrigger = new InputTracker<float>(poller.Field("rightControllerIndexFloat"), XRNode.RightHand);

        LeftPrimary = new InputTracker<bool>(poller.Field("leftControllerPrimaryButton"), XRNode.LeftHand);
        RightPrimary = new InputTracker<bool>(poller.Field("rightControllerPrimaryButton"), XRNode.RightHand);

        LeftSecondary = new InputTracker<bool>(poller.Field("leftControllerSecondaryButton"), XRNode.LeftHand);
        RightSecondary = new InputTracker<bool>(poller.Field("rightControllerSecondaryButton"), XRNode.RightHand);

        LeftStickClick = new InputTracker<bool>(pollerExt.Field("leftControllerStickButton"), XRNode.LeftHand);
        RightStickClick = new InputTracker<bool>(pollerExt.Field("rightControllerStickButton"), XRNode.RightHand);

        LeftStickAxis = new InputTracker<Vector2>(pollerExt.Field("leftControllerStickAxis"), XRNode.LeftHand);
        RightStickAxis = new InputTracker<Vector2>(pollerExt.Field("rightControllerStickAxis"), XRNode.RightHand);

        _inputTrackers =
        [
            LeftGrip, RightGrip,
            LeftTrigger, RightTrigger,
            LeftPrimary, RightPrimary,
            LeftSecondary, RightSecondary,
            LeftStickClick, RightStickClick,
            LeftStickAxis, RightStickAxis
        ];
    }

    internal static void Update()
    {
        ControllerInputPollerExt.Instance.Update();
        foreach (var input in _inputTrackers) input.UpdateValues();
    }

    // https://github.com/developer9998/Bark/blob/76e893492845b620c6567be68f6e14bf0cbd49ba/Interaction/GestureTracker.cs#L387
    private class ControllerInputPollerExt
    {
        public static ControllerInputPollerExt Instance;

        public bool leftControllerStickButton, rightControllerStickButton;

        public Vector2 leftControllerStickAxis, rightControllerStickAxis;

        public ControllerInputPollerExt()
        {
            Instance = this;
        }

        public void Update()
        {
            if (GameUtility.IsSteam)
            {
                leftControllerStickButton = SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
                rightControllerStickButton = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
                leftControllerStickAxis = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.axis;
                rightControllerStickAxis = SteamVR_Actions.gorillaTag_RightJoystick2DAxis.axis;
                return;
            }

            LeftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out leftControllerStickButton);
            RightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out rightControllerStickButton);
            LeftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftControllerStickAxis);
            RightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightControllerStickAxis);
        }
    }
}
