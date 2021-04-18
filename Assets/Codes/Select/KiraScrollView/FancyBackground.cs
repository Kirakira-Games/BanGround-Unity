﻿using AudioProvider;
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

    Dictionary<string, Texture2D> _cachedBackgrounds = new Dictionary<string, Texture2D>();

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

    int prevSid = -1, currentSid = -1, nextSid = -1;
    bool firstSelect = true;

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

            if (se != null && !firstSelect)
                se.PlayOneShot();

            firstSelect = false;
        }

        selectManager.SetPreviewVolume(currentSid == selectManager.CurrentPlayingSid ? 1.0f - (Mathf.Abs(position - 0.5f) * 4) : 0);
        material.SetFloat(Uniform.Progress, position);
        background.material = material;
    }
}
