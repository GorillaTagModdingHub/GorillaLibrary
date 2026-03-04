using GorillaLibrary.Events.System;
using System;

namespace GorillaLibrary.Events
{
    public class RoomEvents
    {
        public class RoomJoinedEvent : IEvent { 
            public static void Initialize()
            {
                NetworkSystem.Instance.OnMultiplayerStarted += () => GorillaLibrary.Instance.Bus.Publish(new RoomJoinedEvent());
            }
        }
        public class RoomLeftEvent : IEvent
        {
            public static void Initialize()
            {
                NetworkSystem.Instance.OnReturnedToSinglePlayer += () => GorillaLibrary.Instance.Bus.Publish(new RoomLeftEvent());
            }
        }
    }
}
