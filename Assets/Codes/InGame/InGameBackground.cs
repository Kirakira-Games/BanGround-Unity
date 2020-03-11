using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class InGameBackground : MonoBehaviour
{
    //[SerializeField] private Texture defaultTex;
    [SerializeField] private Material bgSkybox;
    private Material cacheMat = null;

    private void Start()
    {
        Color color = new Color(LiveSetting.bgBrightness, LiveSetting.bgBrightness, LiveSetting.bgBrightness);
        Material mat = Instantiate(bgSkybox);
        mat.SetColor("_Tint", color);
        RenderSettings.skybox = mat;
        cacheMat = mat;

        SceneManager.sceneUnloaded += UpdateBG;
    }

    private void UpdateBG(Scene s)
    {
        if (cacheMat != null)
            RenderSettings.skybox = cacheMat;
    }

    public void SetBcakground(string path)
    {
        if (!File.Exists(path))
        {
            RenderSettings.skybox = bgSkybox;
            return;
        }
        else StartCoroutine(GetAndSetBG(path));
    }

    IEnumerator GetAndSetBG(string path)
    {
        path = "file://" + path;
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(path))
        {
            yield return webRequest.SendWebRequest();
            Texture tex = DownloadHandlerTexture.GetContent(webRequest);
            Color color = new Color(LiveSetting.bgBrightness, LiveSetting.bgBrightness, LiveSetting.bgBrightness);
            float ratio = tex.width / (float)tex.height;

            Material mat = Instantiate(bgSkybox);
            mat.SetTexture("_MainTex", tex); 
            mat.SetColor("_Tint", color);
            mat.SetFloat("_TexRatio", ratio);

            RenderSettings.skybox = mat;
            cacheMat = mat;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= UpdateBG;
    }
}
