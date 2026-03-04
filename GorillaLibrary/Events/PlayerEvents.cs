using GorillaLibrary.Events.System;

namespace GorillaLibrary.Events
{
    public class PlayerEvents
    {

        public class PlayerEnteredRoomEvent(NetPlayer player) : IEvent
        {
            public NetPlayer Player { get; private set; } = player;
        }

        public class PlayerLeftRoomEvent(NetPlayer player) : IEvent
        {
            public NetPlayer Player { get; private set; } = player;
        }

        public class PlayerNameChangedEvent(NetPlayer player, string nickName) : IEvent
        {
            public NetPlayer Player { get; private set; } = player;
            public string Nickname { get; private set; } = nickName;
        }

        public class GameOverlayActivationEvent(bool isGameOverlayActive) : IEvent
        {
            public bool IsGameOverlayActive { get; private set; } = isGameOverlayActive;
        }

    }
}
