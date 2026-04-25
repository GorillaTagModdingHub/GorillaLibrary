using GorillaLibrary.Extensions;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

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

    public static GameObject[] Objects => ZoneManagement.GetField<GameObject[]>("allObjects");

    public static ZoneData GetZoneData(GTZone zone)
    {
        ZoneManagement instance = ZoneManagement;
        MethodInfo method = AccessTools.Method(typeof(ZoneManagement), "GetZoneData");
        return (ZoneData)method.Invoke(instance, [zone]);
    }
}
