using GorillaLibrary.Events.System;
using UnityEngine;

namespace GorillaLibrary.Events
{
    public class RigEvents
    {
        public class RigAddedEvent(VRRig rig, NetPlayer player) : IEvent
        {
            public VRRig Rig { get; private set; } = rig;
            public NetPlayer Player { get; private set; } = player;
        }

        public class RigRemovedEvent(VRRig rig) : IEvent {
            public VRRig Rig { get; private set; } = rig;
        }

        public class ColourChangedEvent(VRRig rig, Color color) : IEvent
        {
            public VRRig Rig { get; private set; } = rig;
            public Color Color { get; private set; } = color;
        }
    }
}
