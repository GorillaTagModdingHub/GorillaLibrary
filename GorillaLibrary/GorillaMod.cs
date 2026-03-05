using GorillaLibrary.Events.System;
using MelonLoader;

namespace GorillaLibrary;

public abstract class GorillaMod : MelonMod
{
    public static readonly EventBus Bus = new();

    public virtual bool Toggleable => true;

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (!Toggleable || _enabled == value)
                return;

            _enabled = value;

            if (_enabled)
                EnableInternal();
            else
                DisableInternal();
        }
    }

    public void Toggle()
    {
        if (!Toggleable) return;
        Enabled = !Enabled;
    }

    private void EnableInternal()
    {
        Bus.Subscribe(this);

        MelonEvents.OnUpdate.Subscribe(OnUpdate);
        MelonEvents.OnLateUpdate.Subscribe(OnLateUpdate);
        MelonEvents.OnFixedUpdate.Subscribe(OnFixedUpdate);
        MelonEvents.OnGUI.Subscribe(OnGUI);

        OnEnable();
    }

    private void DisableInternal()
    {
        Bus.Unsubscribe(this);

        MelonEvents.OnUpdate.Unsubscribe(OnUpdate);
        MelonEvents.OnLateUpdate.Unsubscribe(OnLateUpdate);
        MelonEvents.OnFixedUpdate.Unsubscribe(OnFixedUpdate);
        MelonEvents.OnGUI.Unsubscribe(OnGUI);

        OnDisable();
    }

    public override void OnInitializeMelon()
    {
        if (Enabled)
            EnableInternal();
        OnInitialize();
    }

    public override void OnEarlyInitializeMelon()
    {
        OnEarlyInitialize();
    }

    public override void OnLateInitializeMelon()
    {
        OnLateInitialize();
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }

    protected virtual new void OnUpdate() { }
    protected virtual new void OnLateUpdate() { }
    protected virtual new void OnFixedUpdate() { }
    protected virtual new void OnGUI() { }

    protected virtual void OnInitialize() { }
    protected virtual void OnEarlyInitialize() { }
    protected virtual void OnLateInitialize() { }
}