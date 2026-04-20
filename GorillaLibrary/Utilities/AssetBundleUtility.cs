using GorillaLibrary.Extensions;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaLibrary.Utilities;

public static class AssetBundleUtility
{
    public static AssetBundle LoadBundle(Assembly assembly, string path)
    {
        Stream stream = assembly.GetManifestResourceStream(path);
        AssetBundle bundle = AssetBundle.LoadFromStream(stream);
        stream.Close();
        return bundle;
    }

    public static async Task<AssetBundle> LoadBundleAsync(Assembly assembly, string path)
    {
        Stream stream = assembly.GetManifestResourceStream(path);
        AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(stream);
        await request.AsAwaitable();
        stream.Close();
        return request.assetBundle;
    }

    public static async Task<T> LoadAssetAsync<T>(AssetBundle bundle, string name) where T : Object
    {
        AssetBundleRequest request = bundle.LoadAssetAsync<T>(name);
        await request.AsAwaitable();
        return request.asset as T;
    }

    public static async Task<T[]> LoadAssetsWithSubAssetsAsync<T>(AssetBundle bundle, string name) where T : Object
    {
        AssetBundleRequest request = bundle.LoadAssetWithSubAssetsAsync<T>(name);
        await request.AsAwaitable();
        return [.. request.allAssets.Cast<T>()];
    }
}
