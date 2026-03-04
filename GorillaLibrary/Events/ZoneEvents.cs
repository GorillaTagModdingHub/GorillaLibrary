using GorillaLibrary.Events.System;
using System.Collections.Generic;

namespace GorillaLibrary.Events
{
    public class ZoneEvents
    {
        public class ZonesChangedEvent(IEnumerable<GTZone> activeZones) : IEvent
        {
            public IEnumerable<GTZone> ActiveZones { get; private set; } = activeZones;
        }
    }
}
