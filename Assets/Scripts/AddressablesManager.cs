using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesManager : MonoBehaviour
{
    //[SerializeField] private AssetReference refPrefab;
    [SerializeField] private GameObject pee;

    private List<object> updatedKeys = new List<object>() { "Cube", "1", "2", "5" };

    private void Start()
    {
        // 清除 Cube 预制体的缓存
        //Addressables.ClearDependencyCacheAsync("Cube");


        //Addressables.ClearDependencyCacheAsync("Cube");
        //Addressables.ClearDependencyCacheAsync("Sphere");

        //CheckUpdates();
        Addressables.InstantiateAsync("Cube");
    }

    private void Update()
    {
        //InvokeRepeating("GenerateCube", 1f, 1f);
    }

    private void GenerateCube()
    {
        Addressables.InstantiateAsync("Cube");
    }

    public void UpdateGame()
    {
        StartCoroutine(UpdateGame_Coroutine());
    }

    private IEnumerator UpdateGame_Coroutine()
    {
        var initHandle = Addressables.InitializeAsync(false);
        yield return initHandle;

        if (initHandle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Error! Initialization failed!");
            Addressables.Release(initHandle); // Release the handle after checking it
            yield break;
        }


        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Error! Updates check failed!");
            yield break;
        }

        if (checkHandle.Result.Count > 0)
        {
            Debug.Log("Updates found!");

            var logUpdateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            yield return logUpdateHandle;

            if (logUpdateHandle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Error! Catalog update failed!");
                yield break;
            }

            //var locators = logUpdateHandle.Result;
            foreach (var locator in logUpdateHandle.Result)
            {
                updatedKeys.AddRange(locator.Keys);
            }

            var sizeHandle = Addressables.GetDownloadSizeAsync(updatedKeys);
            yield return sizeHandle;

            if (sizeHandle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Error! Unable to get download file size!");
                yield break;
            }

            long downloadSize = sizeHandle.Result;
            Debug.Log($"download size: {downloadSize}");

            if (downloadSize > 0)
            {
                var downloadHandle = Addressables.DownloadDependenciesAsync(updatedKeys, false);
                while (!downloadHandle.IsDone)
                {
                    if (downloadHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed)
                    {
                        Debug.LogError("Error! Updates download failed!");
                        yield break;
                    }

                    float downloadProgress = downloadHandle.PercentComplete;
                    Debug.Log($"Download progress: {downloadProgress}%");

                    yield return null;
                }

                if (downloadHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("Download Completed!");
                }
            }
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Instantiate prefab"))
        {
            //Addressables.InstantiateAsync(refPrefab, Random.insideUnitSphere * 3, Quaternion.identity);
            Addressables.InstantiateAsync("Sphere", Random.insideUnitSphere * 3, Quaternion.identity);
            Addressables.LoadAssetAsync<Texture2D>("4").Completed += (handle) =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    pee.GetComponent<MeshRenderer>().material.mainTexture = handle.Result;
                }

            };

            Addressables.LoadAssetsAsync<Texture2D>("images", (texture) =>
            {
                Debug.Log("image loaded!");
            });
        }
    }



    //will only work if auto check update on game startup is disabled in addressables settings
    private void CheckUpdates()
    {
        Debug.Log("cnm");
        //check for updates
        Addressables.CheckForCatalogUpdates(false).Completed += (check) =>
        {
            Debug.Log(check.Result.Count);
            foreach (var result in check.Result)
            {
                Debug.Log(result);
            }
            if (check.Result.Count > 0)
            {
                Addressables.UpdateCatalogs(check.Result, false).Completed += (handle) =>
                {
                    foreach (var asset in handle.Result)
                    {
                        updatedKeys.AddRange(asset.Keys);
                        Debug.Log(asset.Keys.ToString());
                        Debug.Log("key added!");
                    }

                    Addressables.Release(handle);
                    Addressables.Release(check);
                };
            }
        };

        foreach (var element in updatedKeys)
        {
            Debug.Log(element);
        }

        StartCoroutine(DownloadUpdates_Coroutine());

    }

    private IEnumerator DownloadUpdates_Coroutine()
    {
        var downloadSizeHandle = Addressables.GetDownloadSizeAsync(updatedKeys);
        yield return downloadSizeHandle;

        Debug.Log("download size: " + downloadSizeHandle.Result);
        if (downloadSizeHandle.Result > 0)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(updatedKeys, Addressables.MergeMode.Union);
            while (!downloadHandle.IsDone)
            {
                DownloadStatus info = downloadHandle.GetDownloadStatus();

                print(info.Percent);
                print(info.DownloadedBytes + " / " + info.TotalBytes);
            }
            Debug.Log("download completed");
            Addressables.Release(downloadHandle);
        }
    }
}
