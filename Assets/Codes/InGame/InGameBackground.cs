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

    public void SetBackground(string path)
    {
        if (!KiraFilesystem.Instance.Exists(path))
        {
            RenderSettings.skybox = bgSkybox;
            return;
        }
        else
        {
            var tex = KiraFilesystem.Instance.ReadTexture2D(path);

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
