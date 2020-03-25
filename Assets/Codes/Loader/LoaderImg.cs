using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoaderImg : MonoBehaviour
{
    //private Texture[] loaderSprite
    const string ListUri = "http://47.100.29.0:1144/";
    const string MemeUri = "http://47.100.29.0:1144/meme/";
    string TempSav = "";
    private RawImage image;

    void Start()
    {
        image = GetComponent<RawImage>();
        TempSav = Application.persistentDataPath + "/MemeTemp/";

        if (!Directory.Exists(TempSav))
            Directory.CreateDirectory(TempSav);
        StartCoroutine(DownloadImg());

    }

    IEnumerator DownloadImg()
    {
        var list = UnityWebRequest.Get(ListUri);
        string listString = "";
        yield return list.SendWebRequest();
        if (list.isNetworkError || list.isHttpError)
        {
            Debug.LogError("Get meme list failed : " + list.error);
            var LocalList = new DirectoryInfo(TempSav).GetFiles();
            if (LocalList.Length > 0)
            {
                var randomIndex = Random.Range(0, LocalList.Length);
                var imgReq = UnityWebRequest.Get(LocalList[randomIndex].FullName);
                var dht = new DownloadHandlerTexture(true);
                imgReq.downloadHandler = dht;
                yield return imgReq.SendWebRequest();
                if (imgReq.isHttpError)
                    Debug.LogError("Get Local meme img failed : " + list.error);
                else
                {
                    image.texture = dht.texture;
                    image.SetNativeSize();
                }
            }
        }
        else
        {
            //Debug.Log(list.downloadHandler.text);
            listString = list.downloadHandler.text;
            string[] imgs = listString.Replace(" ","").Replace("\n", "").Split(',');
            var randomIndex = Random.Range(0, imgs.Length);
            var img = imgs[randomIndex];
            if (!File.Exists(TempSav + img))
            {
                var imgReq = UnityWebRequest.Get(MemeUri + img);
                var dht = new DownloadHandlerTexture(true);
                imgReq.downloadHandler = dht;
                yield return imgReq.SendWebRequest();
                if (imgReq.isHttpError)
                    Debug.LogError("Get meme img failed : " + list.error);
                else
                {
                    image.texture = dht.texture;
                    image.SetNativeSize();
                    File.WriteAllBytes(TempSav + img, dht.data);
                }
            }
            else
            {
                var imgReq = UnityWebRequest.Get(TempSav + img);
                var dht = new DownloadHandlerTexture(true);
                imgReq.downloadHandler = dht;
                yield return imgReq.SendWebRequest();
                if (imgReq.isHttpError)
                    Debug.LogError("Get meme img failed : " + list.error);
                else
                {
                    image.texture = dht.texture;
                    image.SetNativeSize();
                }
            }
        }
    }

}
