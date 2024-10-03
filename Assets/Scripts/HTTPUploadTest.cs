using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class HTTPUploadTest : MonoBehaviour
{
    private string serverAddress = "http://127.0.0.1/HTTPFileServer";

    [ContextMenu("Upload bundles to http server")]
    public void UploadBundlesToHttpServer()
    {
        GetFileFromHTTPServer("File.txt");
    }

    private IEnumerator UploadFileToHTTPServer(string _filePath)
    {
        byte[] buffer = File.ReadAllBytes(_filePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", buffer, "images");

        UnityWebRequest request = UnityWebRequest.Put($"{serverAddress}/AssetBundles/PC", buffer);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"{_filePath}\nuploaded!");
        }
        else
        {
            Debug.LogError($"{_filePath}\nupload failed!\n" + request.downloadHandler.text);
        }

    }

    private void WebClientTest(string _filePath)
    {
        WebClient client = new WebClient();

        byte[] responseArray = client.UploadFile($"{serverAddress}/AssetBundles/PC", _filePath);
    }

    private void GetFileFromHTTPServer(string _fileName)
    {
        StartCoroutine(GetFileFromHTTPServer_Coroutine(_fileName));
    }

    private IEnumerator GetFileFromHTTPServer_Coroutine(string _fileName)
    {
        UnityWebRequest request = UnityWebRequest.Get($"{serverAddress}/{_fileName}");
        yield return request.SendWebRequest();

        byte[] buffer = request.downloadHandler.data;
        File.WriteAllBytes("E:\\GameProject\\pee.txt", buffer);
    }
}
