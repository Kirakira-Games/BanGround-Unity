using AudioProvider;
using FancyScrollView;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class FancyBackground : MonoBehaviour
{
    private const int BACKGROUND_CACHE_SIZE = 10;

    [Inject]
    private IDataLoader dataLoader;
    [Inject]
    private IResourceLoader resourceLoader;
    [Inject]
    private IAudioManager audioManager;
    [Inject]
    private SelectManager selectManager;

    public RawImage background = default;
    public KiraScrollView scrollView = default;
    public Scroller scroller = default;
    public Text title = default;
    public Text artist = default;
    public Texture2D defaultTexture = default;
    public TextAsset changeSound = default;

    private ISoundEffect se;
    private Material material;
    private PriorityQueue<string> mLRUCache = new PriorityQueue<string>();
    private int mLRUTimestamp = 0;

    static class Uniform
    {
        public static readonly int Texture1 = Shader.PropertyToID("_Tex1");
        public static readonly int Texture2 = Shader.PropertyToID("_Tex2");
        public static readonly int Texture3 = Shader.PropertyToID("_Tex3");
        public static readonly int AspectRatios = Shader.PropertyToID("_TexRatios");
        public static readonly int Progress = Shader.PropertyToID("_Progress");
    }

    async void Start()
    {
        material = Instantiate(background.material);

        scrollView.OnMove += UpdatePosition;

        if(changeSound != null)
        {
            se = await audioManager.PrecacheSE(changeSound.bytes);
        }
    }

    private int prevSid = -1, currentSid = -1, nextSid = -1;
    private bool firstSelect = true;

    private Texture2D GetBackground(string path)
    {
        if (string.IsNullOrEmpty(path))
            return defaultTexture;

        var tex = resourceLoader.LoadTextureFromFs(path);
        if (tex == null)
        {
            Debug.LogWarning($"{path} not exists! Fallback to default texture.");
            return defaultTexture;
        }

        mLRUCache.Push(path, mLRUTimestamp++);
        while (mLRUCache.Count > BACKGROUND_CACHE_SIZE)
        {
            string toRemove = mLRUCache.Pop();
            resourceLoader.UnloadTexture(toRemove);
        }

        return tex;
    }

    void UpdatePosition(float pos)
    {
        if (dataLoader.chartList.Count == 0)
            return;

        int N = dataLoader.chartList.Count;
        pos = (pos % N + N) % N;

        int current = Mathf.RoundToInt(pos);

        if (current >= dataLoader.chartList.Count)
            current -= 1;

        int last = current - 1;
        int next = current + 1;

        var position = (pos - current) / 2 + 0.5f;

        if (last < 0)
            last = dataLoader.chartList.Count - 1;

        if (next >= dataLoader.chartList.Count)
            next = 0;

        var s1 = dataLoader.chartList[last];
        var s2 = dataLoader.chartList[current];
        var s3 = dataLoader.chartList[next];

        if (prevSid != s1.sid || currentSid != s2.sid || nextSid != s3.sid)
        {
            prevSid = s1.sid;
            currentSid = s2.sid;
            nextSid = s3.sid;

            var b1 = dataLoader.GetBackgroundPath(s1.sid, true).Item1;
            var b2 = dataLoader.GetBackgroundPath(s2.sid, true).Item1;
            var b3 = dataLoader.GetBackgroundPath(s3.sid, true).Item1;

            var tex1 = GetBackground(b1);
            var tex2 = GetBackground(b2);
            var tex3 = GetBackground(b3);

            material.SetTexture(Uniform.Texture1, tex1);
            material.SetTexture(Uniform.Texture2, tex2);
            material.SetTexture(Uniform.Texture3, tex3);

            material.SetVector(Uniform.AspectRatios, new Vector4(tex1.width / (float)tex1.height, tex2.width / (float)tex2.height, tex3.width / (float)tex3.height, 0));

            var mheader = dataLoader.GetMusicHeader(dataLoader.chartList[current].mid);
            title.text = mheader.title;
            artist.text = mheader.artist;

            if (se != null && !firstSelect)
                se.PlayOneShot();

            firstSelect = false;
        }

        selectManager.SetPreviewVolume(currentSid == selectManager.CurrentPlayingSid ? 1.0f - (Mathf.Abs(position - 0.5f) * 4) : 0);
        material.SetFloat(Uniform.Progress, position);
        background.material = material;
    }
}
