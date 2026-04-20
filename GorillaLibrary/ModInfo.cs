using GorillaLibrary.Models;
using MelonLoader;
using System;
using System.Linq;

namespace GorillaLibrary;

public class ModInfo
{
    public MelonMod Mod { get; set; }
    public GameModeWrapper[] Gamemodes { get; set; }
    public Action<string> OnGamemodeJoin { get; set; }
    public Action<string> OnGamemodeLeave { get; set; }

    public override string ToString()
    {
        return $"{Mod.Info.Name} [{string.Join(", ", Gamemodes.Select(x => x.DisplayName))}]";
    }
}
