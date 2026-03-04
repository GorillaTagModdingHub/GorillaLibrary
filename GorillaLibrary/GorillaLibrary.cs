using GorillaLibrary.Events;
using System;

namespace GorillaLibrary
{
    public class GorillaLibrary
    {
        private static readonly Lazy<GorillaLibrary> _instance = 
            new(() => new GorillaLibrary());

        public static GorillaLibrary Instance => _instance.Value;

        public readonly EventBus Bus = new();
    }
}
