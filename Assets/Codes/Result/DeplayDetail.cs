using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeplayDetail : MonoBehaviour
{
    [SerializeField]
    public Image[] bars;
    [SerializeField]
    public float maxHeight = 500;

    void Start()
    {
        int range = NoteUtility.TAP_JUDGE_RANGE[3] * 2 / bars.Length;

        var ranges = Enumerable.Range(1, bars.Length - 1)
            .Select(x => -NoteUtility.TAP_JUDGE_RANGE[3] + x * range)
            .Append(int.MaxValue);

        // make sure at least every range have 1 element so
        // the counts.Count() will equals to bars.Length
        var result = ComboManager.JudgeOffsetResult.AsEnumerable();
        ranges.All(r => (result = result.Append(r)) != null);

        var counts = result    
            .GroupBy(x => ranges.First(r => r >= x))
            .Select(g => g.Count() - 1);

        Debug.Assert(counts.Count() == bars.Length);

        float maxCount = counts.Max();

        float[] heights = counts
            .Select(c => c / maxCount * maxHeight)
            .ToArray();
      
        for(int i = 0; i < bars.Length; ++i) 
        {
            var size = bars[i].rectTransform.sizeDelta;
            size.y = heights[bars.Length - i];

            bars[i].rectTransform.sizeDelta = size;
        }
    }
}
