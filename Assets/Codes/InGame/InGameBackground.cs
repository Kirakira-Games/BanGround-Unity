using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Zenject;
using BanGround;
using BanGround.Game.Mods;

#pragma warning disable 0649
public class InGameBackground : MonoBehaviour, IInGameBackground
{
    [Inject]
    private IModManager modManager;
    //[SerializeField] private Texture defaultTex;
    [SerializeField]
    private Material bgSkybox;
    private Material cacheMat = null;
    private MeshRenderer mesh;
    private VideoPlayer vp;

    [Inject(Id = "r_brightness_bg")]
    KVar r_brightness_bg;

    [Inject]
    private IFileSystem fs;

    [Inject]
    private IResourceLoader resourceLoader;

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        Color color = new Color(r_brightness_bg, r_brightness_bg, r_brightness_bg);
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

    public void SetBackground(Texture2D tex)
    {
        Color color = new Color(r_brightness_bg, r_brightness_bg, r_brightness_bg);

        vp.enabled = false;

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

    public void SetBackground(string path, int type)
    {
        if (path == null || (type == 0 && !fs.FileExists(path)))
        {
            //RenderSettings.skybox = bgSkybox;
            mesh.sharedMaterial = bgSkybox;
            return;
        }
        else
        {
            Color color = new Color(r_brightness_bg, r_brightness_bg, r_brightness_bg);

            if (type == 0)
            {
                vp.enabled = false;
                var tex = resourceLoader.LoadTextureFromFs(path);


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
                vp.targetCameraAlpha = r_brightness_bg;
                vp.playbackSpeed = modManager.SpeedCompensationSum;
            }
        }
    }

    public void playVideo()
    {
        if (vp.isActiveAndEnabled)
            vp.Play();
    }

    public void pauseVideo()
    {
        if (vp.isActiveAndEnabled)
            vp.Pause();
    }

    public void stopVideo()
    {
        if (vp.isActiveAndEnabled)
            vp.Stop();
    }

    public void seekVideo(double sec)
    {
        if (vp.isActiveAndEnabled)
            vp.time = sec;
    }

    private void Update()
    {
        //print(vp.time);
    }
    //private void OnDestroy()
    //{
    //    SceneManager.sceneUnloaded -= UpdateBG;
    //}
}
