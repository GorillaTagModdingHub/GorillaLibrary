using GorillaLibrary.Events.System;

namespace GorillaLibrary.Events
{
    public class ServerEvents
    {
        public class MothershipMessageReceivedEvent : IEvent
        {
            public string Title { get; private set; }
            public string Body { get; private set; }

            public MothershipMessageReceivedEvent(string title, string body)
            {
                Title = title;
                Body = body;
            }

            public MothershipMessageReceivedEvent(NotificationsMessageResponse notification)
            {
                Title = notification.Title;
                Body = notification.Body;
            }
        }

    }
}
