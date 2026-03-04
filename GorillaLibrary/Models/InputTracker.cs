using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.XR;

namespace GorillaLibrary.Models;

// https://github.com/developer9998/Bark/blob/76e893492845b620c6567be68f6e14bf0cbd49ba/Interaction/GestureTracker.cs#L347
public abstract class InputTracker
{
    internal bool pressed, wasPressed;
    internal Vector3 vector3Value;
    internal Quaternion quaternionValue;
    internal XRNode node;
    internal string name;
    internal Traverse traverse;
    internal Action<InputTracker> OnPressed, OnReleased;

    public abstract void UpdateValues();
}

// https://github.com/developer9998/Bark/blob/76e893492845b620c6567be68f6e14bf0cbd49ba/Interaction/GestureTracker.cs#L360
public class InputTracker<T> : InputTracker
{
    public InputTracker(Traverse traverse, XRNode node)
    {
        this.traverse = traverse;
        this.node = node;
    }

    public T GetValue()
    {
        return traverse.GetValue<T>();
    }
    public override void UpdateValues()
    {
        wasPressed = pressed;

        if (typeof(T) == typeof(bool))
            pressed = traverse.GetValue<bool>();
        else if (typeof(T) == typeof(float))
            pressed = traverse.GetValue<float>() > .5f;

        if (!wasPressed && pressed)
            OnPressed?.Invoke(this);
        if (wasPressed && !pressed)
            OnReleased?.Invoke(this);
    }
}
