using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

public class KirakiraWebRequest<Resp>
{
    public Resp resp;
    public bool isNetworkError;

    public Coroutine Get(string url)
    {
        return KirakiraWebRequestObject.instance.StartCoroutine(RunGet(url));
    }

    private IEnumerator RunGet(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError | webRequest.isHttpError)
            {
                isNetworkError = true;
            }
            else
            {
                resp = JsonConvert.DeserializeObject<Resp>(webRequest.downloadHandler.text);
            }
        }
    }

    public Coroutine Post<Req>(string url, Req req)
    {
        return KirakiraWebRequestObject.instance.StartCoroutine(RunPost(url, req));
    }

    private IEnumerator RunPost<Req>(string url, Req req)
    {
        var reqJson = JsonConvert.SerializeObject(req);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(reqJson);
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError | webRequest.isHttpError)
            {
                isNetworkError = true;
            }
            else
            {
                resp = JsonConvert.DeserializeObject<Resp>(webRequest.downloadHandler.text);
            }
        }
    }
}

