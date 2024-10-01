using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AssetBundleUpdateManager : MonoBehaviour
{
    public static AssetBundleUpdateManager instance;

    //Dictionary<bundleName, BundleInfo>
    private Dictionary<string, BundleInfo> remoteBundleDictionary = new Dictionary<string, BundleInfo>();
    private Dictionary<string, BundleInfo> localBundleDictionary = new Dictionary<string, BundleInfo>();


    //List<bundleName>
    private List<string> bundleToDownloadList = new List<string>();

    private string persistentDataPath;

    [SerializeField] private GameObject downloadProgressBarPrefab;
    private GameObject downloadProgressBar;
    private Slider slider;
    [SerializeField] private Transform Canvas;
    [SerializeField] private GameObject loadingScreen;

    private class BundleInfo
    {
        public string name;
        public long size;
        public string md5;

        public BundleInfo(string _name, string _size, string _md5)
        {
            name = _name;
            size = long.Parse(_size);
            md5 = _md5;
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

        persistentDataPath = Application.persistentDataPath;
        Debug.Log(persistentDataPath);

        UpdateGame();
    }

    private void Start()
    {
    }


    public void UpdateGame()
    {
        remoteBundleDictionary.Clear();
        localBundleDictionary.Clear();
        bundleToDownloadList.Clear();

        DownloadAssetBundleComparisonFileFromFTPServer((downloadCompleted) =>
        {
            if (downloadCompleted)
            {
                //Debug.Log("Loading remote comparison file info...");
                GetRemoteAssetBundleComparisonFileInfo();
                //Debug.Log("Remote comparison file info loaded!");

                //Debug.Log("Loading local comparison file info...");
                GetLocalAssetBundleComparisonFileInfo(() =>
                {
                    //UpdateOutdatedBundles();
                    FigureOutBundlesToDownload();

                    DownloadNecessaryAssetBundlesFromFTPServer((downloadCompleted) =>
                    {
                        if (downloadCompleted)
                        {
                            DeleteDeprecatedBundles();

                            //create local bundle comparison file
                            //CreateBundleComparisonFile(persistentDataPath);

                            //before delete remote comparison file,
                            //maybe check if the new generated comparison file is same as the remote file
                            //to check if the downloaded bundles are intact?
                            //DeleteTempRemoteBundleComparisonFile();

                            RenameTempRemoteComparisonFileToLocal();

                            //AssetBundleManager.instance.LoadResourceAsync<TextAsset>("luascripts", "LuaScript.txt", (lua) =>
                            //{
                            //    Debug.Log("Lua scripts loaded");
                            //});
                        }
                        else
                        {
                            Debug.LogError("Network Error! Failed to update bundles!");
                        }
                    });

                });
                //Debug.Log("Localcomparison file info loaded!");

            }
            else
            {
                Debug.LogError("Network Error! Failed to download remote bundle comparison file!");
            }
        });
    }

    private void DeleteDeprecatedBundles()
    {
        foreach (var bundleInfo in localBundleDictionary)
        {
            if (!remoteBundleDictionary.ContainsKey(bundleInfo.Key))
            {
                File.Delete(persistentDataPath + "/" + bundleInfo.Key);
                Debug.Log("deprecataed bundles deleted");
            }
        }
        //Debug.Log("5");
    }

    private void RenameTempRemoteComparisonFileToLocal()
    {
        if (File.Exists(persistentDataPath + "/BundleComparisonFile_Remote.txt"))
        {
            File.Delete(persistentDataPath + "/BundleComparisonFile.txt");
            File.Move(persistentDataPath + "/BundleComparisonFile_Remote.txt", persistentDataPath + "/BundleComparisonFile.txt");
        }
    }

    private void DeleteTempRemoteBundleComparisonFile()
    {
        if (File.Exists(persistentDataPath + "/BundleComparisonFile_Remote.txt"))
        {
            File.Delete(persistentDataPath + "/BundleComparisonFile_Remote.txt");
            Debug.Log("Temp remote bundle comparison file deleted");
        }

        //Debug.Log("7");
    }

    private void FigureOutBundlesToDownload()
    {
        foreach (var bundle_remote in remoteBundleDictionary)
        {
            if (localBundleDictionary.TryGetValue(bundle_remote.Key, out BundleInfo localBundleInfo))
            {
                if (string.Compare(localBundleInfo.md5, bundle_remote.Value.md5) != 0)
                {
                    //download this asset bundle
                    //DownloadFileFromFTPServer(bundle_remote.Key, $"{persistentDataPath}/{bundle_remote.Key}");
                    bundleToDownloadList.Add(bundle_remote.Key);
                    Debug.Log($"local md5: {localBundleInfo.md5}\nremote md5: {bundle_remote.Value.md5}");
                    Debug.Log($"Added {bundle_remote.Key} to download list");
                }
            }
            else
            {
                //download this asset bundle
                //DownloadFileFromFTPServer(bundle_remote.Key, $"{persistentDataPath}/{bundle_remote.Key}");
                bundleToDownloadList.Add(bundle_remote.Key);
                Debug.Log($"Added {bundle_remote.Key} to download list");
            }
        }
        //Debug.Log("3");
    }

    //private async void UpdateOutdatedBundles()
    //{
    //    await Task.Run(() =>
    //    {
    //        foreach (var bundle_remote in remoteBundleDictionary)
    //        {
    //            if (localBundleDictionary.TryGetValue(bundle_remote.Key, out BundleInfo localBundleInfo))
    //            {
    //                if (localBundleInfo.md5 != bundle_remote.Value.md5)
    //                {
    //                    //download this asset bundle
    //                    DownloadFileFromFTPServer(bundle_remote.Key, $"{persistentDataPath}/{bundle_remote.Key}");
    //                }
    //            }
    //            else
    //            {
    //                //download this asset bundle
    //                DownloadFileFromFTPServer(bundle_remote.Key, $"{persistentDataPath}/{bundle_remote.Key}");
    //            }
    //        }
    //    });
    //}

    public async void DownloadNecessaryAssetBundlesFromFTPServer(System.Action<bool> OnDownloadProcessEnd)
    {
        //foreach (var search in remoteBundleDictionary)
        //{
        //    bundleToDownloadList.Add(search.Key);
        //}

        if (bundleToDownloadList.Count == 0)
        {
            Debug.Log("Already up-to-date!");
        }

        string downloadLocation = Application.persistentDataPath + "/";
        int downloadProgress = 1;
        List<string> successfulDownloadedBundleList = new List<string>();
        bool currentBundleDownloadCompleted = false;
        int downloadRetrialAmount = 5;

        //if error occurred, will retry download for certain times
        while (bundleToDownloadList.Count > 0 && downloadRetrialAmount > 0)
        {
            downloadProgress = 1;

            if (downloadProgressBar == null)
            {
                downloadProgressBar = Instantiate(downloadProgressBarPrefab, Canvas);
                slider = downloadProgressBar.GetComponent<Slider>();
                slider.maxValue = bundleToDownloadList.Count;
                loadingScreen.SetActive(true);
            }

            foreach (var bundleToDownload in bundleToDownloadList)
            {
                currentBundleDownloadCompleted = false;

                Debug.Log($"Download Progress: {downloadProgress} / {bundleToDownloadList.Count}");
                slider.value = downloadProgress;

                await Task.Run(() =>
                {
                    currentBundleDownloadCompleted = DownloadFileFromFTPServer(bundleToDownload, downloadLocation + bundleToDownload);
                });

                if (currentBundleDownloadCompleted)
                {
                    downloadProgress++;
                    successfulDownloadedBundleList.Add(bundleToDownload);
                }

            }

            //remove the downloaded bundles from the toDownloadList
            foreach (var successfulDownloadedBundle in successfulDownloadedBundleList)
            {
                bundleToDownloadList.Remove(successfulDownloadedBundle);
            }

            //if didn't completed downloading all the bundles
            //will retry download
            downloadRetrialAmount--;
            if (bundleToDownloadList.Count > 0)
            {
                Debug.Log($"Error! Retrying download...");
            }
        }

        if (bundleToDownloadList.Count > 0)
        {
            Debug.LogError("Network Error! Failed to update all the bundles");
        }
        else
        {
            Debug.Log("All bundles are updated!");
        }

        //Debug.Log("4");
        bool allBundlesDownloadCompleted = false;
        if (bundleToDownloadList.Count == 0)
        {
            allBundlesDownloadCompleted = true;
        }

        Destroy(downloadProgressBar);
        loadingScreen.SetActive(false);

        OnDownloadProcessEnd?.Invoke(allBundlesDownloadCompleted);

        //same as
        //if (OnDownloadProcessEnded != null)
        //{
        //    OnDownloadProcessEnded(allBundlesDownloadCompleted);
        //}
    }

    public async void DownloadAssetBundleComparisonFileFromFTPServer(System.Action<bool> OnDownloadProcessEnd)
    {
        int retrialAmount = 5;

        bool downloadCompleted = false;
        while (!downloadCompleted && retrialAmount > 0)
        {
            downloadCompleted = false;
            await Task.Run(() =>
            {
                downloadCompleted = DownloadFileFromFTPServer("BundleComparisonFile.txt", persistentDataPath + "/BundleComparisonFile_Remote.txt");
            });
            retrialAmount--;
        }

        OnDownloadProcessEnd?.Invoke(downloadCompleted);

        //same as
        //if (OnDownloadProcessEnded != null)
        //{
        //    OnDownloadProcessEnded(downloadCompleted);
        //}
    }

    private void GetAssetBundleComparisonFileInfo(string _comparisonFilePath, ref Dictionary<string, BundleInfo> _outputDictionary)
    {
        //string[] infos = File.ReadAllLines(_comparisonFilePath);
        string temp = File.ReadAllText(_comparisonFilePath);
        string[] infos = temp.Split('\n');
        foreach (var element in infos)
        {
            string[] currentBundleInfo = element.Split(" ");

            //0 for name, 1 for size, 2 for md5
            BundleInfo bundleInfo = new BundleInfo(currentBundleInfo[0], currentBundleInfo[1], currentBundleInfo[2]);
            _outputDictionary.Add(currentBundleInfo[0], bundleInfo);
            //Debug.Log($"{currentBundleInfo[0]}\n{currentBundleInfo[1]}\n{currentBundleInfo[2]}\n");
        }

        //Debug.Log("Comparison file info loaded");
    }

    private void GetAssetBundleComparisonFileInfo_MultiplePlatform(string _comparisonFilePath, Dictionary<string, BundleInfo> _outputDictionary, System.Action OnInfoGet)
    {
        StartCoroutine(GetAssetBundleComparisonFileInfo_MultiplePlatform_Coroutine(_comparisonFilePath, localBundleDictionary, OnInfoGet));
    }

    private IEnumerator GetAssetBundleComparisonFileInfo_MultiplePlatform_Coroutine(string _comparisonFilePath, Dictionary<string, BundleInfo> _outputDictionary, System.Action OnInfoGet)
    {
        UnityWebRequest req = UnityWebRequest.Get(new Uri(_comparisonFilePath));
        yield return req.SendWebRequest();

        //delete the last chang line symbol
        string temp = req.downloadHandler.text;
        string[] infos = temp.Split('\n');
        foreach (var element in infos)
        {
            string[] currentBundleInfo = element.Split(" ");
            //0 for name, 1 for size, 2 for md5
            BundleInfo bundleInfo = new BundleInfo(currentBundleInfo[0], currentBundleInfo[1], currentBundleInfo[2]);
            _outputDictionary.Add(currentBundleInfo[0], bundleInfo);
            //Debug.Log($"{currentBundleInfo[0]}\n{currentBundleInfo[1]}\n{currentBundleInfo[2]}\n");
        }

        OnInfoGet?.Invoke();
    }

    private void GetRemoteAssetBundleComparisonFileInfo()
    {
        GetAssetBundleComparisonFileInfo(persistentDataPath + "/BundleComparisonFile_Remote.txt", ref remoteBundleDictionary);
    }

    private void GetLocalAssetBundleComparisonFileInfo(System.Action OnGetInfo)
    {
        //UnityWebRequest req = UnityWebRequest.Get("pee");
        //req.SendWebRequest();
        //Debug.Log(req.downloadHandler.text);
        if (File.Exists(persistentDataPath + "/BundleComparisonFile.txt"))
        {
            //GetAssetBundleComparisonFileInfo(Application.persistentDataPath + "/BundleComparisonFile.txt", ref localBundleDictionary);
            GetAssetBundleComparisonFileInfo_MultiplePlatform("file:///" + Application.persistentDataPath + "/BundleComparisonFile.txt", localBundleDictionary, OnGetInfo);
        }
        else if (File.Exists(Application.streamingAssetsPath + "/BundleComparisonFile.txt"))
        {
            string path =
                #if UNITY_ANDROID
                    Application.streamingAssetsPath;
                #else
                    "file:///" + Application.streamingAssetsPath;
                #endif
            //GetAssetBundleComparisonFileInfo(Application.streamingAssetsPath + "/BundleComparisonFile.txt", ref localBundleDictionary);
            GetAssetBundleComparisonFileInfo_MultiplePlatform(path + "/BundleComparisonFile.txt", localBundleDictionary, OnGetInfo);
        }
        else
        {
            Debug.Log("Didn't find existed local bundle comparison file, will download all the bundles from server");
            OnGetInfo?.Invoke();
        }

        //Debug.Log("2");
    }

    private bool DownloadFileFromFTPServer(string _fileName, string _downloadLocation)
    {
        try
        {
            string platform =
#if UNITY_IOS
                "IOS";
#elif UNITY_ANDROID
                "Android";
#else
                "PC";
#endif

            FtpWebRequest req = FtpWebRequest.Create(new Uri($"ftp://127.0.0.1/AssetBundles/{platform}/" + _fileName)) as FtpWebRequest;

            NetworkCredential credential = new NetworkCredential("Megumi", "114514");
            req.Credentials = credential;

            req.Proxy = null;
            req.KeepAlive = false;

            req.Method = WebRequestMethods.Ftp.DownloadFile;

            req.UseBinary = true;

            //download should be response
            FtpWebResponse resp = req.GetResponse() as FtpWebResponse;
            Stream downloadStream = resp.GetResponseStream();

            using (FileStream fs = new FileStream(_downloadLocation, FileMode.Create))
            {
                byte[] bytes = new byte[2048];
                int contentLength = downloadStream.Read(bytes, 0, bytes.Length);

                while (contentLength > 0)
                {
                    fs.Write(bytes, 0, contentLength);

                    contentLength = downloadStream.Read(bytes, 0, bytes.Length);
                }
            }

            downloadStream.Close();
            Debug.Log($"{_fileName} download succeeded");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"{_fileName} download failed\n" + e.Message);
            return false;
        }
    }

    private void CreateBundleComparisonFile(string _bundlesDirectoryPath)
    {
        DirectoryInfo directory = Directory.CreateDirectory(_bundlesDirectoryPath);
        FileInfo[] fileInfos = directory.GetFiles();

        StringBuilder sb = new StringBuilder();

        foreach (var fileInfo in fileInfos)
        {
            if (fileInfo.Extension == "")
            {
                Debug.Log(fileInfo.Name);
                sb.Append($"{fileInfo.Name} {fileInfo.Length} {GetMD5(fileInfo.FullName)}");
                sb.AppendLine();
            }
        }

        //delete the last empty line
        sb.Remove(sb.Length - 1, 1);

        Debug.Log(sb.ToString());

        File.WriteAllText($"{_bundlesDirectoryPath}\\BundleComparisonFile.txt", sb.ToString());
        Debug.Log("Created Asset Bundle Comparison File");
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        //Debug.Log("6");
    }

    private string GetMD5(string _filePath)
    {
        using (FileStream fs = new FileStream(_filePath, FileMode.Open))
        {
            //MD5 md5 = new MD5();
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5Info = md5.ComputeHash(fs);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < md5Info.Length; i++)
            {
                sb.Append(md5Info[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
