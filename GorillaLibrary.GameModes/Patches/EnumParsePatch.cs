using GorillaGameModes;
using GorillaLibrary.GameModes.Models;
using GorillaLibrary.GameModes.Utilities;
using HarmonyLib;
using System;
using System.Reflection;

namespace GorillaLibrary.GameModes.Patches;

[HarmonyPatch]
internal class EnumParsePatch
{
    public static MethodBase TargetMethod()
    {
        return typeof(Enum)
            .GetMethod(nameof(Enum.Parse), BindingFlags.Public | BindingFlags.Static, null, [typeof(string), typeof(bool)], null)
            ?.MakeGenericMethod(typeof(GameModeType));
    }

    public static bool Prefix(string value, ref object __result)
    {
        if (GameModeUtility.GetGamemodeFromId(value) is Gamemode gamemode)
        {
            __result = gamemode.BaseGamemode.GetValueOrDefault(GameModeType.Infection);
            return false;
        }

        EnumData<GameModeType> shared = EnumData<GameModeType>.Shared;
        __result = shared.NameToEnum.TryGetValue(value, out var gameMode) ? gameMode : GameModeType.Infection;

        return false;
    }
}
