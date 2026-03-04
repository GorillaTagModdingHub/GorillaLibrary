using GorillaLibrary.Events.System;
using MelonLoader;

namespace GorillaLibrary;

public class GorillaMod : MelonMod
{
    protected virtual void Awake() { }
    protected virtual void Start() { }
    protected virtual void Update() { }

    public static readonly EventBus Bus = new();

    public sealed override void OnInitializeMelon()
    {
        Bus.Subscribe(this);
        Awake();
    }

    public sealed override void OnLateInitializeMelon() => Start();

    public sealed override void OnUpdate() => Update();
}