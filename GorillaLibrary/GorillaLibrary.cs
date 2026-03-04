using GorillaLibrary.Events;
using MelonLoader;
using System;

namespace GorillaLibrary
{
    public class GorillaMod : MelonMod
    {
        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }

        protected EventBus Bus => GorillaLibrary.Instance.Bus;

        public sealed override void OnInitializeMelon()
        {
            Bus.Subscribe(this);
            Awake();
        }

        public sealed override void OnLateInitializeMelon()
        {
            Start();
        }

        public sealed override void OnUpdate() { Update(); }
    }

    public class GorillaLibrary
    {
        private static readonly Lazy<GorillaLibrary> _instance = 
            new(() => new GorillaLibrary());

        public static GorillaLibrary Instance => _instance.Value;

        public readonly EventBus Bus = new();
    }
}
