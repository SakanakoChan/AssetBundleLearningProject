using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AssetBundleTools
{
    private static string PCBundlePath = Application.dataPath + "\\ResourcesForHotFix\\AssetBundles\\PC";

    //[MenuItem("AssetBundleTools/Create Bundle Comparison File (Remote)")]
    public static void CreateBundleComparisonFile_Remote()
    {
        CreateBundleComparisonFile(Application.dataPath + "\\ResourcesForHotFix\\AssetBundles\\PC");
    }

    public static void CreateBundleComparisonFile(string _bundlesDirectoryPath)
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
    public static void UploadAllBundlesAndComparisonFileToServer()
    {
        DirectoryInfo directory = Directory.CreateDirectory(PCBundlePath);
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

    private static async void UploadFileToFTPServer(string _filePath, string _fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                //specify request uri
                FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://127.0.0.1/AssetBundles/PC/" + _fileName)) as FtpWebRequest;

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

    private static string GetMD5(string _filePath)
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
    private static void CopyBundlesToStreamingAssetsFolderAndGenerateLocalBundleComparisonFile()
    {
        UnityEngine.Object[] selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Deep);

        if (selectedAssets.Length == 0)
        {
            Debug.Log("None asset is selected");
            return;
        }

        foreach(var asset in selectedAssets)
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
