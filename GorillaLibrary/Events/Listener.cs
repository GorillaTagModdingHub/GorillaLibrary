using System;

namespace GorillaLibrary.Events
{
    public class Listener<T> where T : IEvent
    {
        public Action<T> Handler { get; }

        public Listener(Action<T> handler)
        {
            Handler = handler;
        }

        public Listener(Func<T, object> handler)
        {
            Handler = e => handler(e);
        }

        public static implicit operator Listener<T>(Action<T> handler)
            => new(handler);
    }
}
