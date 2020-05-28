using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using UniRx.Async;

public class KirakiraWebRequest<Resp>
{
    public Resp resp;
    public bool isNetworkError;

    public async UniTask<Resp> Get(string url)
    {
        return await RunGet(url);
    }

    private async UniTask<Resp> RunGet(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            await webRequest.SendWebRequest();
            if (webRequest.isNetworkError | webRequest.isHttpError)
            {
                isNetworkError = true;
            }
            else
            {
                resp = JsonConvert.DeserializeObject<Resp>(webRequest.downloadHandler.text);
            }
        }
        return resp;
    }

    public async UniTask<Resp> Post<Req>(string url, Req req)
    {
        return await RunPost(url, req);
    }

    private async UniTask<Resp> RunPost<Req>(string url, Req req)
    {
        var reqJson = JsonConvert.SerializeObject(req);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(reqJson);
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            await webRequest.SendWebRequest();
            if (webRequest.isNetworkError | webRequest.isHttpError)
            {
                isNetworkError = true;
            }
            else
            {
                resp = JsonConvert.DeserializeObject<Resp>(webRequest.downloadHandler.text);
            }
        }
        return resp;
    }
}

