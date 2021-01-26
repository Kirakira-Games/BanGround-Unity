using BanGround;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Newtonsoft.Json;

public class ParticleSequence : MonoBehaviour
{
    class ParticleInfo
    {
        [JsonProperty("texture")]
        public string TextureName;
        [JsonIgnore]
        public Texture2D Texture;
        [JsonProperty("rows")]
        public int Rows;
        [JsonProperty("cols")]
        public int Cols;
        [JsonProperty("frames")]
        public int Frames;
        [JsonProperty("fps")]
        public int FPS;
        [JsonProperty("size")]
        public Vector2 Size;
    }

    static Dictionary<TapEffectType, ParticleInfo> ParticleInfos;
    static class Uniform
    {
        public static readonly int Texture = Shader.PropertyToID("_MainTex");
        public static readonly int Param = Shader.PropertyToID("_Param");
        public static readonly int Scale = Shader.PropertyToID("_Scale");
    }

    Vector3 globalScale;
    MeshRenderer meshRenderer;
    Material mat;

    bool isLoop = false;
    bool isPlaying = false;
    int curFrame = 0;
    int frameCount = 1;
    int fps = 60;
    Vector4 _params = Vector4.zero;

    ParticleInfo curParticle;

    void Start()
    {
        globalScale = transform.localScale;

        meshRenderer = GetComponent<MeshRenderer>();
        mat = meshRenderer.material;

        meshRenderer.enabled = false;
    }

    public static void SetParticlePath(string path, IFileSystem fs)
    {
        var json = KiraPath.Combine(path, "info.json");

        ParticleInfos = JsonConvert.DeserializeAnonymousType(fs.GetFile(json).ReadAsString(), ParticleInfos);

        foreach (var (_, info) in ParticleInfos)
        {
            var texFullPath = KiraPath.Combine(path, info.TextureName);
            info.Texture = fs.GetFile(texFullPath).ReadAsTexture();
        }
    }

    public void Play(TapEffectType type, bool isLoop = false)
    {
        curParticle = ParticleInfos[type];

        isPlaying = true;
        fps = curParticle.FPS;
        frameCount = curParticle.Frames;
        this.isLoop = isLoop;

        transform.localScale = globalScale * curParticle.Size;

        _params.x = 1.0f / curParticle.Cols;
        _params.y = 1.0f / curParticle.Rows;

        mat.SetTexture(Uniform.Texture, curParticle.Texture);
        mat.SetVector(Uniform.Param, _params);
        mat.SetVector(Uniform.Scale, globalScale * curParticle.Size);

        StartCoroutine(UpdateFrame());
    }

    public void Stop()
    {
        isPlaying = false;
    }

    IEnumerator UpdateFrame()
    {
        curFrame = 0;
        float delayTime = 1.0f / fps;

        while(isPlaying)
        {
            yield return new WaitForSecondsRealtime(delayTime);
            ++curFrame;

            if (curFrame >= frameCount)
            {
                if (isLoop)
                    curFrame = 0;
                else
                    isPlaying = false;
            }
        }

        curFrame = -1;
    }

    int lastFrame = 0;

    void Update()
    {
        if (isPlaying)
        {
            if (!meshRenderer.enabled)
                meshRenderer.enabled = true;

            if (curFrame != lastFrame)
            {
                lastFrame = curFrame;

                int col = curFrame % curParticle.Cols;
                int row = curParticle.Rows - 1 - (curFrame / curParticle.Rows);

                _params.z = col;
                _params.w = row;

                mat.SetVector(Uniform.Param, _params);
            }
        }
        else if(meshRenderer.enabled)
        {
            meshRenderer.enabled = false;
        }
    }
}
