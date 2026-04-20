using GorillaLibrary.Utilities;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GorillaLibrary.Models;

public class AssetLoaderSync
{
    private readonly Assembly _assembly;

    private readonly string _path;

    private readonly Dictionary<string, object> _cache = [];

    private AssetBundle _bundle;

    public AssetLoaderSync(string path)
    {
        _assembly = Assembly.GetCallingAssembly();
        _path = path;
    }

    public AssetLoaderSync(Assembly assembly, string path)
    {
        _assembly = assembly;
        _path = path;
    }

    public AssetBundle GetBundle()
    {
        if (_bundle == null)
        {
            return _bundle = AssetBundleUtility.LoadBundle(_assembly, _path);
        }

        return _bundle;
    }

    public T LoadAsset<T>(string name) where T : Object
    {
        if (_cache.ContainsKey(name) && _cache[name] is Object cachedAsset) return cachedAsset as T;

        T asset = GetBundle().LoadAsset<T>(name);
        _cache.TryAdd(name, asset);

        return asset;
    }

    public T[] LoadAssetWithSubAssets<T>(string name) where T : Object
    {
        if (_cache.ContainsKey(name) && _cache[name] is T[] cachedArray) return cachedArray;

        T[] assets = GetBundle().LoadAssetWithSubAssets<T>(name);
        _cache.TryAdd(name, assets);

        return assets;
    }
}
