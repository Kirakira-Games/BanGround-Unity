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
            //没网的时候
            Debug.LogError("Get meme list failed : " + list.error);
            var LocalList = new DirectoryInfo(TempSav).GetFiles();
            if (LocalList.Length > 0)
            {
                var randomIndex = Random.Range(0, LocalList.Length);
                Debug.LogWarning("file://" + LocalList[randomIndex].FullName);
                var imgReq = UnityWebRequest.Get("file://"+LocalList[randomIndex].FullName);
                var dht = new DownloadHandlerTexture(true);
                imgReq.downloadHandler = dht;
                yield return imgReq.SendWebRequest();
                if (imgReq.isHttpError)
                    Debug.LogError("Get Local meme img failed : " + list.error);
                else
                {
                    image.texture = dht.texture;
                    AdjLimitImageSize();
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
                    AdjLimitImageSize();
                    File.WriteAllBytes(TempSav + img, dht.data);
                }
            }
            else
            {
                //已经保存的
                Debug.LogWarning("file://" + TempSav + img);
                var imgReq = UnityWebRequest.Get("file://"+ TempSav + img);
                var dht = new DownloadHandlerTexture(true);
                imgReq.downloadHandler = dht;
                yield return imgReq.SendWebRequest();
                if (imgReq.isHttpError)
                    Debug.LogError("Get meme img failed : " + list.error);
                else
                {
                    image.texture = dht.texture;
                    AdjLimitImageSize();
                }
            }
        }
    }

    void AdjLimitImageSize()
    {
        float aspectR = image.texture.height / (float)image.texture.width;
        if (aspectR >= 1f)
            image.rectTransform.sizeDelta = new Vector2(300f / aspectR, 300f);
        else
            image.rectTransform.sizeDelta = new Vector2(300f, 300f * aspectR);
    }
}
