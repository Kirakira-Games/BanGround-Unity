using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class InGameBackground : MonoBehaviour
{
    //[SerializeField] private Texture defaultTex;
    [SerializeField] 
    private Material bgSkybox;
    private Material cacheMat = null;
    private MeshRenderer mesh;
    private VideoPlayer vp;

    public static InGameBackground instance;

    private void Awake()
    {
        instance = this;
        mesh = GetComponent<MeshRenderer>();
        Color color = new Color(LiveSetting.bgBrightness, LiveSetting.bgBrightness, LiveSetting.bgBrightness);
        Material mat = Instantiate(bgSkybox);
        mat.SetColor("_Tint", color);
        //RenderSettings.skybox = mat;
        mesh.sharedMaterial = mat;
        cacheMat = mat;
        vp = GetComponent<VideoPlayer>();
        //vp.Stop();
        //SceneManager.sceneUnloaded += UpdateBG;
    }




    //private void UpdateBG(Scene s)
    //{
    //    if (cacheMat != null)
    //        RenderSettings.skybox = cacheMat;
    //}

    public void SetBackground(string path, int type)
    {
        if (path == null || (type == 0 && !KiraFilesystem.Instance.Exists(path)))
        {
            //RenderSettings.skybox = bgSkybox;
            mesh.sharedMaterial = bgSkybox;
            return;
        }
        else
        {
            Color color = new Color(LiveSetting.bgBrightness, LiveSetting.bgBrightness, LiveSetting.bgBrightness);

            if (type == 0)
            {
                vp.enabled = false;
                var tex = KiraFilesystem.Instance.ReadTexture2D(path);


                float ratio = tex.width / (float)tex.height;

                Material mat = Instantiate(bgSkybox);
                mat.SetTexture("_MainTex", tex);
                mat.SetColor("_Tint", color);
                mat.SetFloat("_TexRatio", ratio);

                //RenderSettings.skybox = mat;
                mesh.sharedMaterial = mat;
                Destroy(cacheMat);
                cacheMat = mat;
            }
            else
            {
                vp.url = path;
                vp.Prepare();
                playVideo();
                pauseVideo();
                mesh.enabled = false;
                vp.targetCameraAlpha = LiveSetting.bgBrightness;
                foreach(ModBase m in LiveSetting.attachedMods)
                {
                    if (m is AudioMod)
                        vp.playbackSpeed = (m as AudioMod).SpeedCompensation;
                }
            }
        }
    }

    public void playVideo() => vp.Play();
    public void pauseVideo() => vp.Pause();
    public void stopVideo() => vp.Stop();

    public void seekVideo(double sec) => vp.time = sec;

    private void Update()
    {
        //print(vp.time);
    }
    //private void OnDestroy()
    //{
    //    SceneManager.sceneUnloaded -= UpdateBG;
    //}
}
