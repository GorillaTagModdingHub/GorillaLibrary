using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaLibrary.Utilities;

public static class TextureLoaderUtility
{
    private static readonly Dictionary<string, TaskCompletionSource<Texture2D>> cache = [];

    public static async Task<Texture2D> LoadTexture(string url, TextureFormat format = TextureFormat.RGB24)
    {
        if (cache.TryGetValue(url, out TaskCompletionSource<Texture2D> completionSource))
        {
            Texture2D texture = completionSource.Task.IsCompleted ? completionSource.Task.Result : await completionSource.Task;
            return texture;
        }

        completionSource = new();
        cache.Add(url, completionSource);

        using UnityWebRequest request = UnityWebRequest.Get(url);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        await operation;

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = new(2, 2, format, false);
            texture.LoadImage(request.downloadHandler.data);
            completionSource.TrySetResult(texture);
            return texture;
        }

        return null;
    }
}
