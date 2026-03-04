using GorillaTag;
using System.Collections.Generic;

namespace GorillaLibrary.Events;

public class ZoneEvents
{
    public static DelegateListProcessor<IEnumerable<GTZone>> OnZonesChanged;
}
