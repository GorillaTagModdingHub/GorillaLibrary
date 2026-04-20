using GorillaGameModes;
using HarmonyLib;
using MelonLoader;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GorillaLibrary.Utilities;

public static class PlayerUtility
{
    private static readonly Dictionary<string, (GetAccountInfoResult accountInfo, DateTime cacheTime)> accountInfoCache = [];

    public static GetAccountInfoResult GetAccountInfo(string userId, Action<GetAccountInfoResult> onAccountInfoRecieved, double maxCacheTime = double.MaxValue)
    {
        if (accountInfoCache.ContainsKey(userId) && (DateTime.Now - accountInfoCache[userId].cacheTime).TotalMinutes < maxCacheTime)
            return accountInfoCache[userId].accountInfo;

        if (!PlayFabClientAPI.IsClientLoggedIn())
            throw new InvalidOperationException("PlayFab Client must be logged in to post the account info request");

        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest
        {
            PlayFabId = userId
        }, accountInfo =>
        {
            if (accountInfoCache.ContainsKey(userId)) accountInfoCache[userId] = (accountInfo, DateTime.Now);
            else accountInfoCache.Add(userId, (accountInfo, DateTime.Now));
            onAccountInfoRecieved?.Invoke(accountInfo);
        }, error =>
        {
            Melon<Mod>.Logger.Error(error.GenerateErrorReport());
        });

        return null;
    }

    public static Task<GetAccountInfoResult> GetAccountInfo(string userId, double maxCacheTime = double.MaxValue)
    {
        TaskCompletionSource<GetAccountInfoResult> completionSource = new();

        GetAccountInfoResult result = GetAccountInfo(userId, newResult =>
        {
            if (completionSource.Task.IsCompleted) return;
            completionSource.SetResult(newResult);
        }, maxCacheTime);

        if (result != null) completionSource.SetResult(result);

        return completionSource.Task;
    }

    public static bool IsTagged(NetPlayer player, GorillaTagManager tagManager)
    {
        return tagManager.currentInfected?.Contains(player) ?? false;
    }

    public static bool IsTagger(NetPlayer player, GorillaTagManager tagManager)
    {
        return tagManager.currentIt == player;
    }

    public static bool IsTagged(NetPlayer player, GorillaHuntManager huntManager)
    {
        return huntManager.currentHunted?.Contains(player) ?? false;
    }

    public static bool IsParticipant(NetPlayer player)
    {
        return (bool)AccessTools.Method(typeof(GameMode), "CanParticipate").Invoke(null, [player]);
    }
}
