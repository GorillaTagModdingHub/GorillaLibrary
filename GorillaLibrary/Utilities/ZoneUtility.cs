using HarmonyLib;

namespace GorillaLibrary.Utilities;

public static class ZoneUtility
{
    public static ZoneManagement ZoneManagement
    {
        get
        {
            if (ZoneManagement.instance == null) ZoneManagement.FindInstance();
            return ZoneManagement.instance;
        }
    }

    public static ZoneData[] Zones
    {
        get
        {
            ZoneManagement instance = ZoneManagement;
            return (ZoneData[])AccessTools.Field(typeof(ZoneManagement), "zones").GetValue(instance);
        }
    }
}
