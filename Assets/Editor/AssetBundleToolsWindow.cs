using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleToolsWindow : EditorWindow
{
    private int selectedPlatformIndex = 0;
    private string[] platforms = { "PC", "Android", "IOS" };
    //default address
    private string resourceServerAddress = "ftp://127.0.0.1";

    [MenuItem("AssetBundleTools/Open tools window")]
    private static void OpenToolsWindow()
    {
        AssetBundleToolsWindow toolsWindow = EditorWindow.GetWindowWithRect(typeof(AssetBundleToolsWindow), new Rect(0, 0, 550, 300)) as AssetBundleToolsWindow;
        toolsWindow.Show();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 15), "Platform");
        selectedPlatformIndex = GUI.Toolbar(new Rect(10, 30, 300, 30), selectedPlatformIndex, platforms);

        GUI.Label(new Rect(10, 70, 300, 15), "Resouce server address");
        resourceServerAddress = GUI.TextField(new Rect(10, 90, 300, 20), resourceServerAddress);

        if (GUI.Button(new Rect(10, 120, 250, 40), "Create bundle comparison file (remote)"))
        {
            CreateBundleComparisonFile_Remote();
        }

        if (GUI.Button(new Rect(10, 170, 525, 40), "Copy selected assets to StreamingAsstes folder and generate local bundle comparison file"))
        {
            CopySelectedBundlesToStreamingAssetsFolderAndGenerateLocalBundleComparisonFile();
        }

        if (GUI.Button(new Rect(10, 240, 285, 50), "Upload bundles and comparison file to server"))
        {
            if (resourceServerAddress.Contains("ftp"))
            {
                UploadAllBundlesAndComparisonFileToFTPServer();

            }
            else if (resourceServerAddress.Contains("http"))
            {
                //UploadAllBundlesAndComparisonFileToHTTPServer();
                DownloadFileFromHTTPServer("File.txt", "E:\\GameProject\\File.txt");
            }
        }
    }

    public void CreateBundleComparisonFile_Remote()
    {
        CreateBundleComparisonFile(Application.dataPath + $"\\ResourcesForHotFix\\AssetBundles\\{platforms[selectedPlatformIndex]}");
    }

    public void CreateBundleComparisonFile(string _bundlesDirectoryPath)
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
        AssetDatabase.Refresh();
    }

    //[MenuItem("AssetBundleTools/Upload Bundles and Comparison File To Server")]
    public void UploadAllBundlesAndComparisonFileToFTPServer()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + $"\\ResourcesForHotFix\\AssetBundles\\{platforms[selectedPlatformIndex]}");
        FileInfo[] fileInfos = directory.GetFiles();

        foreach (var fileInfo in fileInfos)
        {
            //empty extension is the asset bundle
            //.txt extension is lua script
            if (fileInfo.Extension == "" || fileInfo.Extension == ".txt")
            {
                //Debug.Log(fileInfo.Name);
                UploadFileToFTPServer(fileInfo.FullName, fileInfo.Name);
            }
        }
    }


    public void UploadAllBundlesAndComparisonFileToHTTPServer()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + $"\\ResourcesForHotFix\\AssetBundles\\{platforms[selectedPlatformIndex]}");
        FileInfo[] fileInfos = directory.GetFiles();

        foreach (var fileInfo in fileInfos)
        {
            //empty extension is the asset bundle
            //.txt extension is lua script
            if (fileInfo.Extension == "" || fileInfo.Extension == ".txt")
            {
                //Debug.Log(fileInfo.Name);
                UploadFileToHTTPServer(fileInfo.FullName, fileInfo.Name);
            }
        }
    }

    private async void DownloadFileFromHTTPServer(string _fileName, string _downloadLocation)
    {
        await Task.Run(() =>
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(new Uri(resourceServerAddress + "/" + _fileName)) as HttpWebRequest;
                request.Method = WebRequestMethods.Http.Get;
                request.Proxy = null;
                request.KeepAlive = false;

                var response = request.GetResponse();
                Stream downloadStream = response.GetResponseStream();

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
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        });
    }

    private async void UploadFileToHTTPServer(string _filePath, string _fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(new Uri($"{resourceServerAddress}/AssetBundles/{platforms[selectedPlatformIndex]}/" + _fileName)) as HttpWebRequest;
                request.Method = WebRequestMethods.Http.Put;
                request.Proxy = null;
                request.KeepAlive = false;

                Stream uploadStream = request.GetRequestStream();

                using (FileStream fs = File.OpenRead(_filePath))
                {
                    byte[] buffer = new byte[2048];
                    int contentLength = fs.Read(buffer, 0, buffer.Length);

                    while (contentLength > 0)
                    {
                        uploadStream.Write(buffer, 0, contentLength);

                        contentLength = fs.Read(buffer, 0, buffer.Length);
                    }
                }

                uploadStream.Close();
                Debug.Log($"[{_fileName}] uploaded!");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        });
    }

    private async void UploadFileToFTPServer(string _filePath, string _fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                //specify request uri
                FtpWebRequest req = FtpWebRequest.Create(new Uri($"{resourceServerAddress}/AssetBundles/{platforms[selectedPlatformIndex]}/" + _fileName)) as FtpWebRequest;

                //specify request credential (username and password)
                NetworkCredential credential = new NetworkCredential("Megumi", "114514");
                req.Credentials = credential;

                //enable ssl authentication to make sure server won't say Use AUTH first
                //req.EnableSsl = true;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //req.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequired;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //req.UsePassive = true;

                //X509Certificate certificate = X509Certificate.CreateFromCertFile(Application.dataPath + "\\FTPcertificate");
                //X509CertificateCollection certificateCollection = new X509CertificateCollection();
                //certificateCollection.Add(certificate);

                //req.ClientCertificates = certificateCollection;

                //specify request proxy settings
                req.Proxy = null;

                //close the connection after request is completed
                req.KeepAlive = false;

                //speicfy the operation command as upload file
                req.Method = WebRequestMethods.Ftp.UploadFile;

                //specify the transmition type as binary
                req.UseBinary = true;

                Stream uploadStream = req.GetRequestStream();

                using (FileStream fs = File.OpenRead(_filePath))
                {
                    byte[] bytes = new byte[2048];
                    int contentLength = fs.Read(bytes, 0, bytes.Length);

                    while (contentLength > 0)
                    {
                        uploadStream.Write(bytes, 0, contentLength);

                        contentLength = fs.Read(bytes, 0, bytes.Length);
                    }
                }

                uploadStream.Close();

                Debug.Log(_fileName + " Upload succeeded");
            }
            catch (Exception ex)
            {
                Debug.LogError(_fileName + " Upload failed\n" + ex.Message);
            }
        });
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

    //[MenuItem("AssetBundleTools/Copy Bundles To StreamingAssets Folder And Generate Local Bundle Comparison File")]
    private void CopySelectedBundlesToStreamingAssetsFolderAndGenerateLocalBundleComparisonFile()
    {
        UnityEngine.Object[] selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Deep);

        if (selectedAssets.Length == 0)
        {
            Debug.Log("None asset is selected");
            return;
        }

        foreach (var asset in selectedAssets)
        {
            Debug.Log(asset.name);
            Debug.Log(Application.streamingAssetsPath);
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string assetName = assetPath.Substring(assetPath.LastIndexOf('/'));

            //only takes asset bundle file, which doesn't have any suffixes in file name
            if (assetName.Contains('.'))
            {
                continue;
            }

            //Debug.Log(AssetDatabase.GetAssetPath(asset));
            string copyToPath = Application.streamingAssetsPath + assetName;
            AssetDatabase.CopyAsset(assetPath, copyToPath);
            Debug.Log($"{assetName} copied to location {copyToPath}");
        }

        AssetDatabase.Refresh();

        CreateBundleComparisonFile(Application.streamingAssetsPath);
    }
}
