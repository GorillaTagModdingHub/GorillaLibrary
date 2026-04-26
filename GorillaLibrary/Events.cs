using System;

namespace GorillaLibrary;

public class Events
{
    public static readonly CoreEvents Core = new();

    [Obsolete]
    public static readonly CoreEvents Game = Core;

    public static readonly PlayerEvents Player = new();

    public static readonly RigEvents Rig = new();

    public static readonly RoomEvents Room = new();

    public static readonly ServerEvents Server = new();

    public static readonly ZoneEvents Zone = new();

    public static readonly GameModeEvents GameMode = new();

    public static readonly CosmeticEvents Cosmetics = new();
}
