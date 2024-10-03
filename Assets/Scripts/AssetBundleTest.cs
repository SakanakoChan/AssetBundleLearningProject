using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssetBundleTest : MonoBehaviour
{
    [SerializeField] private Image image;


    // Start is called before the first frame update
    void Start()
    {
        //AssetBundleManager.instance.LoadResourceAsync<TextAsset>("luascripts", "LuaSript.lua.txt", (luaScript) =>
        //{
        //    XLuaManager.instance.env.DoString(luaScript.text);
        //});

        //AssetBundleManager.instance.LoadResourceAsync<TextAsset>("luascripts", "LuaScript.txt", (lua) =>
        //{
        //    Debug.Log("Lua scripts loaded");
        //});

        //UnityEngine.Object cube = AssetBundleManager.instance.LoadResource("model", "Cube") as GameObject;
        //Instantiate(cube);

        //GameObject obj = AssetBundleManager.instance.LoadResource<GameObject>("model", "Cube");
        //Instantiate(obj, Vector3.down, Quaternion.identity);

        //UnityEngine.Object pee = AssetBundleManager.instance.LoadResource("model", "Cube", typeof(GameObject));
        //Instantiate(pee, Vector3.right, Quaternion.identity);

        //AssetBundleManager.instance.LoadResourceAsync("model", "Cube", (obj) =>
        //{
        //    Instantiate(obj, Vector3.up, Quaternion.identity);
        //});

        //AssetBundleManager.instance.LoadResourceAsync("model", "Cube", typeof(GameObject), (obj) =>
        //{
        //    Instantiate(obj, Vector3.down, Quaternion.identity);
        //});

        //AssetBundleManager.instance.LoadResourceAsync<GameObject>("model", "Cube", (obj) =>
        //{
        //    Instantiate(obj, Vector3.right, Quaternion.identity);
        //});

        //AssetBundleManager.instance.UnloadAllBundles();

        //AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + "model");

        ////load dependent bundles according to main bundle
        //AssetBundle mainBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/PC");
        //AssetBundleManifest mainManifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        //string[] dependenciesList = mainManifest.GetAllDependencies("model");
        //foreach (var element in dependenciesList)
        //{
        //    Debug.Log("Loading dependent bundle: " + element);
        //    AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + element);
        //}



        //GameObject obj = ab.LoadAsset<GameObject>("Cube");
        ////GameObject obj = ab.LoadAsset("Cube", typeof(GameObject)) as GameObject;

        //Instantiate(obj);

        ////StartCoroutine(LoadAssetBundle_Coroutine("images", "3260f7cf-14a2-4cb5-9816-8a3c52459717"));

        //AssetBundle.UnloadAllAssetBundles(false);
        ////ab.Unload(false);

        //Debug.Log(NumberInFibArrayAtPosition(-1));
    }

    private IEnumerator LoadAssetBundle_Coroutine(string _bundleName, string _assetName)
    {
        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + _bundleName);
        yield return abcr;

        AssetBundleRequest abr = abcr.assetBundle.LoadAssetAsync<Sprite>(_assetName);
        yield return abr;

        image.sprite = abr.asset as Sprite;
        Debug.Log("Finished!");
    }

    private int NumberInFibArrayAtPosition(int _position)
    {
        if (_position == 1 || _position == 2)
        {
            return 1;
        }
        else if (_position >= 3)
        {
            return NumberInFibArrayAtPosition(_position - 1) + NumberInFibArrayAtPosition(_position - 2);
        }
        else
        {
            throw new Exception("Error: position should be greater than or equal to 1");
        }
    }
}
