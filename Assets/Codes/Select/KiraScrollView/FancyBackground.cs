using BanGround;
using FancyScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class FancyBackground : MonoBehaviour
{
    [Inject]
    IDataLoader dataLoader;

    [Inject]
    IFileSystem fs;

    public RawImage background = default;
    public KiraScrollView scrollView = default;
    public Scroller scroller = default;

    Material material;

    RectTransform rectTransform;

    static Dictionary<string, Texture2D> _cachedBackgrounds = new Dictionary<string, Texture2D>();

    public float MostCenterdCellButShiftedPosition = -1;
    public float MostCenterdCellPosition = -1;
    public int MostCenterdCellIndex = -1;
    public int LastMostCenterdCellIndex = -1;
    public float LastMostCenterdCellPosition = -1;

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
        rectTransform = transform as RectTransform;

        material = Instantiate(background.material);
    }

    int cur_last = -1, cur_current = -1, cur_next = -1;

    void LateUpdate()
    {
        Debug.Assert(dataLoader.chartList.Count > 0);

        if (LastMostCenterdCellPosition != MostCenterdCellButShiftedPosition || LastMostCenterdCellIndex != MostCenterdCellIndex)
        {
            int current = MostCenterdCellIndex;
            int last = current - 1;
            int next = current + 1;

            var position = MostCenterdCellPosition;
            var shiftedPosition = MostCenterdCellButShiftedPosition;

            var len = Mathf.Abs(shiftedPosition - position) * 2;

            position = (position - 0.5f) / len + 0.5f;

            LastMostCenterdCellPosition = MostCenterdCellButShiftedPosition;
            LastMostCenterdCellIndex = MostCenterdCellIndex;

            if (last < 0)
                last = dataLoader.chartList.Count - 1;

            if (next >= dataLoader.chartList.Count)
                next = 0;

            if (cur_last != last || cur_current != current || cur_next != next)
            {
                cur_last = last;
                cur_current = current;
                cur_next = next;

                var s1 = dataLoader.chartList[last];
                var s2 = dataLoader.chartList[current];
                var s3 = dataLoader.chartList[next];

                var b1 = dataLoader.GetBackgroundPath(s1.sid, true).Item1;
                var b2 = dataLoader.GetBackgroundPath(s2.sid, true).Item1;
                var b3 = dataLoader.GetBackgroundPath(s3.sid, true).Item1;

                if (!_cachedBackgrounds.ContainsKey(b1))
                    _cachedBackgrounds.Add(b1, fs.GetFile(b1).ReadAsTexture());

                if (!_cachedBackgrounds.ContainsKey(b2))
                    _cachedBackgrounds.Add(b2, fs.GetFile(b2).ReadAsTexture());

                if (!_cachedBackgrounds.ContainsKey(b3))
                    _cachedBackgrounds.Add(b3, fs.GetFile(b3).ReadAsTexture());

                var tex1 = _cachedBackgrounds[b1];
                var tex2 = _cachedBackgrounds[b2];
                var tex3 = _cachedBackgrounds[b3];

                material.SetTexture(Uniform.Texture1, tex1);
                material.SetTexture(Uniform.Texture2, tex2);
                material.SetTexture(Uniform.Texture3, tex3);

                material.SetVector(Uniform.AspectRatios, new Vector4(tex1.width / (float)tex1.height, tex2.width / (float)tex2.height, tex3.width / (float)tex3.height, 0));
            }

            material.SetFloat(Uniform.Progress, position);
            background.material = material;
        }

        // have to reset this everytime
        MostCenterdCellPosition = -1;
    }
}
