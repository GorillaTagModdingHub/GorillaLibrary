using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GorillaLibrary.Events
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _listeners = [];

        public void Subscribe(object target)
        {
            var props = target.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var prop in props)
            {
                if (!prop.PropertyType.IsGenericType) continue;
                if (prop.PropertyType.GetGenericTypeDefinition() != typeof(Listener<>)) continue;

                var eventType = prop.PropertyType.GetGenericArguments()[0];
                var listenerObj = prop.GetValue(target);

                if (listenerObj == null) continue;

                var handler = listenerObj
                    .GetType()
                    .GetProperty("Handler")
                    .GetValue(listenerObj);

                var actionType = typeof(Action<>).MakeGenericType(eventType);
                var del = Delegate.CreateDelegate(actionType, handler, handler.GetType().GetMethod("Invoke"));

                if (!_listeners.TryGetValue(eventType, out var list))
                {
                    list = [];
                    _listeners[eventType] = list;
                }

                list.Add(del);
            }
        }

        public void Unsubscribe(object target)
        {
            foreach (KeyValuePair<Type, List<Delegate>> kvp in _listeners)
            {
                kvp.Value.RemoveAll(d => d.Target == target);
            }
        }

        public void Publish<TEvent>(TEvent ev) where TEvent : IEvent
        {
            if (_listeners.TryGetValue(typeof(TEvent), out List<Delegate> list))
            {
                foreach (Action<TEvent> del in list.Cast<Action<TEvent>>())
                {
                    del(ev);
                }
            }
        }
    }
}
