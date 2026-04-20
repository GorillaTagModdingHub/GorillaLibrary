using GorillaLibrary.Utilities;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaLibrary.Models;

public class AssetLoaderAsync
{
    private readonly Assembly _assembly;

    private readonly string _path;

    private readonly Dictionary<string, object> _cache = [];

    private AssetBundle _bundle;

    private Task _bundleLoadTask = null;

    private bool _isBundleLoaded = false;

    public AssetLoaderAsync(string path)
    {
        _assembly = Assembly.GetCallingAssembly();
        _path = path;
    }

    public AssetLoaderAsync(Assembly assembly, string path)
    {
        _assembly = assembly;
        _path = path;
    }

    private async Task LoadBundle()
    {
        if (_isBundleLoaded) return;

        _bundle = await AssetBundleUtility.LoadBundleAsync(_assembly, _path);
        _isBundleLoaded = true;
    }

    public async Task<T> LoadAsset<T>(string name) where T : Object
    {
        if (_cache.ContainsKey(name) && _cache[name] is Object cachedAsset) return cachedAsset as T;

        if (!_isBundleLoaded)
        {
            _bundleLoadTask ??= LoadBundle();
            await _bundleLoadTask;
        }

        T asset = await AssetBundleUtility.LoadAssetAsync<T>(_bundle, name);
        _cache.TryAdd(name, asset);

        return asset;
    }

    public async Task<T[]> LoadAssetWithSubAssets<T>(string name) where T : Object
    {
        if (_cache.ContainsKey(name) && _cache[name] is T[] cachedArray) return cachedArray;

        if (!_isBundleLoaded)
        {
            _bundleLoadTask ??= LoadBundle();
            await _bundleLoadTask;
        }

        T[] assets = await AssetBundleUtility.LoadAssetsWithSubAssetsAsync<T>(_bundle, name);
        _cache.TryAdd(name, assets);

        return assets;
    }
}
