using GorillaLibrary.Events.System;

namespace GorillaLibrary.Events
{
    public class RoomEvents
    {
        public class RoomJoinedEvent : IEvent
        {
            public static void Initialize()
            {
                NetworkSystem.Instance.OnMultiplayerStarted += () => GorillaMod.Bus.Publish(new RoomJoinedEvent());
            }
        }
        public class RoomLeftEvent : IEvent
        {
            public static void Initialize()
            {
                NetworkSystem.Instance.OnReturnedToSinglePlayer += () => GorillaMod.Bus.Publish(new RoomLeftEvent());
            }
        }
    }
}
