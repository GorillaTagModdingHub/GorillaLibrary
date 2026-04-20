using GorillaLibrary.Utilities;
using Photon.Realtime;
using PlayFab.ClientModels;
using System;

namespace GorillaLibrary.Extensions;

public static class PlayerExtensions
{
    public static NetPlayer AsNetPlayer(this Player player) => NetworkSystem.Instance.GetPlayer(player.ActorNumber) is NetPlayer netPlayer ? netPlayer : null;

    public static Player GetPlayer(this NetPlayer netPlayer) => netPlayer is PunNetPlayer punNetPlayer ? punNetPlayer.PlayerRef : null;

    public static string GetName(this NetPlayer netPlayer, bool limitLength = true)
    {
        bool isNamePermissionEnabled = KIDManager.CheckFeatureSettingEnabled(EKIDFeatures.Custom_Nametags);
        string nickName = netPlayer.NickName;
        string defaultName = netPlayer.DefaultName;

        string playerName = isNamePermissionEnabled ? string.IsNullOrEmpty(nickName) || string.IsNullOrWhiteSpace(nickName) ? defaultName : nickName : defaultName;
        return limitLength ? playerName.LimitLength(12) : playerName;
    }

    public static GetAccountInfoResult GetAccountInfo(this NetPlayer netPlayer, Action<GetAccountInfoResult> callback, double maxCacheTime = double.MaxValue)
    {
        return PlayerUtility.GetAccountInfo(netPlayer.UserId, result =>
        {
            callback?.Invoke(result);
        }, maxCacheTime);
    }

    public static DateTime GetAccountCreation(this NetPlayer netPlayer, Action<DateTime> callback, double maxCacheTime = double.MaxValue)
    {
        return PlayerUtility.GetAccountInfo(netPlayer.UserId, result =>
        {
            if (result.AccountInfo?.TitleInfo?.Created is not DateTime created) return;
            callback?.Invoke(created);
        }, maxCacheTime).AccountInfo?.TitleInfo?.Created is DateTime created ? created : DateTime.MinValue;
    }

    public static bool GetTutorialCompletion(this NetPlayer player)
    {
        return NetworkSystem.Instance.GetPlayerTutorialCompletion(player.ActorNumber);
    }

    public static bool IsTagged(this NetPlayer player)
    {
        GorillaGameManager gameManager = GorillaGameManager.instance;
        return gameManager != null && player.IsTagged(gameManager);
    }

    public static bool IsTagged(this NetPlayer player, GorillaGameManager gameManager)
    {
        if (gameManager is GorillaHuntManager huntManager)
            return PlayerUtility.IsTagged(player, huntManager);

        if (gameManager is GorillaTagManager tagManager)
            return tagManager.isCurrentlyTag ? PlayerUtility.IsTagger(player, tagManager) : PlayerUtility.IsTagged(player, tagManager);

        return false;
    }

    public static bool IsParticipant(this NetPlayer player)
    {
        return PlayerUtility.IsParticipant(player);
    }
}
