using BanGround;
using FancyScrollView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class FancyBackground : MonoBehaviour
{
    [Inject]
    private IDataLoader dataLoader;

    [Inject]
    private IFileSystem fs;

    public RawImage background = default;
    public KiraScrollView scrollView = default;
    public Scroller scroller = default;
    public Text title = default;
    public Text artist = default;
    public Texture2D defaultTexture = default;

    private Material material;

    static Dictionary<string, Texture2D> _cachedBackgrounds = new Dictionary<string, Texture2D>();

    static class Uniform
    {
        public static readonly int Texture1 = Shader.PropertyToID("_Tex1");
        public static readonly int Texture2 = Shader.PropertyToID("_Tex2");
        public static readonly int Texture3 = Shader.PropertyToID("_Tex3");
        public static readonly int AspectRatios = Shader.PropertyToID("_TexRatios");
        public static readonly int Progress = Shader.PropertyToID("_Progress");
    }

    void Start()
    {
        material = Instantiate(background.material);

        scrollView.OnMove += UpdatePosition;
    }

    int prevSid = -1, currentSid = -1, nextSid = -1;

    void UpdatePosition(float pos)
    {
        if (dataLoader.chartList.Count == 0)
            return;

        float N = dataLoader.chartList.Count;
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

            Texture2D GetCachedBackground(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return defaultTexture;

                if (!_cachedBackgrounds.ContainsKey(path))
                {
                    try
                    {
                        var tex = fs.GetFile(path).ReadAsTexture();
                        _cachedBackgrounds.Add(path, tex);
                    }
                    catch(FileNotFoundException)
                    {
                        Debug.LogWarning($"{path} not exists! Your game file had some issue");

                        return defaultTexture;
                    }

                    if (_cachedBackgrounds.Count > 10)
                    {
                        string keyToRemove = null;

                        foreach (var (key, value) in _cachedBackgrounds)
                        {
                            if (key != b1 && key != b2 && key != b3)
                            {
                                if(value != null)
                                    Destroy(value);
                                    
                                keyToRemove = key;
                                break;
                            }
                        }

                        _cachedBackgrounds.Remove(keyToRemove);
                    }
                }
                    
                return _cachedBackgrounds[path];
            }

            var tex1 = GetCachedBackground(b1);
            var tex2 = GetCachedBackground(b2);
            var tex3 = GetCachedBackground(b3);

            material.SetTexture(Uniform.Texture1, tex1);
            material.SetTexture(Uniform.Texture2, tex2);
            material.SetTexture(Uniform.Texture3, tex3);

            material.SetVector(Uniform.AspectRatios, new Vector4(tex1.width / (float)tex1.height, tex2.width / (float)tex2.height, tex3.width / (float)tex3.height, 0));

            var mheader = dataLoader.GetMusicHeader(dataLoader.chartList[current].mid);
            title.text = mheader.title;
            artist.text = mheader.artist;
        }

        material.SetFloat(Uniform.Progress, position);
        background.material = material;
    }
}
