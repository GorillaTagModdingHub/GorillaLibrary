using GorillaNetworking;

namespace GorillaLibrary.Utilities;

public static class CoreUtility
{
    public static bool Initialized { get; private set; }

    /// <summary>
    /// The platform the Gorilla Tag install is based on (either Oculus PC or Steam)
    /// </summary>
    public static string Platform { get; private set; }

    /// <summary>
    /// Whether the Gorilla Tag install is based on the Steam platform
    /// </summary>
    public static bool IsSteam => Platform == "Steam";

    internal static void Initialize()
    {
        Initialized = true;
        Platform = PlayFabAuthenticator.instance.platform.ToString();
    }
}
