using BepInEx;
using GorillaLibrary.Models;
using System;
using System.Linq;

namespace GorillaLibrary;

public class PluginInfo
{
    public BaseUnityPlugin Plugin { get; set; }
    public GameModeWrapper[] Gamemodes { get; set; }
    public Action<string> OnGamemodeJoin { get; set; }
    public Action<string> OnGamemodeLeave { get; set; }

    public override string ToString()
    {
        return $"{Plugin.Info.Metadata.Name} [{string.Join(", ", Gamemodes.Select(x => x.DisplayName))}]";
    }
}
