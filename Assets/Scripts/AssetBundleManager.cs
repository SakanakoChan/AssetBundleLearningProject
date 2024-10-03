using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleManager : MonoBehaviour
{
    public static AssetBundleManager instance;

    private AssetBundle mainBundle = null;
    private AssetBundleManifest mainManifest = null;
    private string bundlePath => Application.persistentDataPath + "/";

    private Dictionary<string, AssetBundle> bundleDictionary = new Dictionary<string, AssetBundle>();

    private string MainBundleName
    {
        get
        {
#if UNITY_IOS
                return "IOS";
#elif UNITY_ANDROID
                return "Android";
#else
            return "PC";
#endif
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Load Resource
    private void LoadBundle(string _bundleName)
    {
        //load main Asset Bundle and its manifest
        if (mainBundle == null)
        {
            mainBundle = AssetBundle.LoadFromFile(bundlePath + MainBundleName);
            mainManifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        //load all denpendices
        string[] dependencies = mainManifest.GetAllDependencies(_bundleName);
        foreach (var dependentBundleName in dependencies)
        {
            if (!bundleDictionary.ContainsKey(dependentBundleName))
            {
                AssetBundle dependentBundle = AssetBundle.LoadFromFile(bundlePath + dependentBundleName);
                bundleDictionary.Add(dependentBundleName, dependentBundle);
            }
        }

        //load required bundle
        if (!bundleDictionary.ContainsKey(_bundleName))
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath + _bundleName);
            bundleDictionary.Add(_bundleName, bundle);
        }

    }

    public Object LoadResource(string _bundleName, string _assetName)
    {
        LoadBundle(_bundleName);

        //load required asset
        return bundleDictionary[_bundleName].LoadAsset(_assetName);
    }

    public Object LoadResource(string _bundleName, string _assetName, System.Type _type)
    {
        LoadBundle(_bundleName);

        //load required asset
        return bundleDictionary[_bundleName].LoadAsset(_assetName, _type);
    }

    public T LoadResource<T>(string _bundleName, string _assetName) where T : Object
    {
        LoadBundle(_bundleName);

        //load required asset
        return bundleDictionary[_bundleName].LoadAsset<T>(_assetName);
    }
    #endregion

    #region Load Resource Async
    public void LoadResourceAsync(string _bundleName, string _assetName, System.Action<Object> _callback)
    {
        StartCoroutine(LoadResourceAsync_Coroutine(_bundleName, _assetName, _callback));
    }

    public void LoadResourceAsync(string _bundleName, string _assetName, System.Type _type, System.Action<Object> _callback)
    {
        StartCoroutine(LoadResourceAsync_Coroutine(_bundleName, _assetName, _type, _callback));
    }

    public void LoadResourceAsync<T>(string _bundleName, string _assetName, System.Action<T> _callback) where T : Object
    {
        StartCoroutine(LoadResourceAsync_Coroutine<T>(_bundleName, _assetName, _callback));
    }



    private IEnumerator LoadResourceAsync_Coroutine(string _bundleName, string _assetName, System.Action<Object> _callback)
    {
        //yield return new WaitForSeconds(1);
        LoadBundle(_bundleName);

        AssetBundleRequest request = bundleDictionary[_bundleName].LoadAssetAsync(_assetName);
        yield return request;

        _callback(request.asset);
    }

    private IEnumerator LoadResourceAsync_Coroutine(string _bundleName, string _assetName, System.Type _type, System.Action<Object> _callback)
    {
        //yield return new WaitForSeconds(1);
        LoadBundle(_bundleName);

        AssetBundleRequest request = bundleDictionary[_bundleName].LoadAssetAsync(_assetName, _type);
        yield return request;

        _callback(request.asset);
    }

    private IEnumerator LoadResourceAsync_Coroutine<T>(string _bundleName, string _assetName, System.Action<T> _callback) where T : Object
    {
        //yield return new WaitForSeconds(1);
        LoadBundle(_bundleName);

        AssetBundleRequest request = bundleDictionary[_bundleName].LoadAssetAsync<T>(_assetName);
        yield return request;

        if (request.asset == null)
        {
            Debug.LogError("Didn't get requested asset!");
        }

        _callback(request.asset as T);
    }
    #endregion

    #region Unload Bundle
    public void UnloadBundle(string _bundleName)
    {
        if (bundleDictionary.TryGetValue(_bundleName, out AssetBundle value))
        {
            bundleDictionary[_bundleName].Unload(false);
            bundleDictionary.Remove(_bundleName);
        }
    }

    public void UnloadAllBundles()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        bundleDictionary.Clear();

        mainBundle = null;
        mainManifest = null;
    }
    #endregion

}
