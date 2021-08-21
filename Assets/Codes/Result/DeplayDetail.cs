using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeplayDetail : MonoBehaviour
{
    [SerializeField]
    public Image[] bars;
    void Start()
    {
        ComboManager.JudgeOffsetResult.Sort();
        
        const int min = -224;
        const int max =  224;
        
        int range = (max - min) / bars.Length;
        int curIdx = 0;
        int curOffset = min;

        float[] heights = new float[bars.Length];
        float maxHeight = 0;
        
        for(int i = 0; i < ComboManager.JudgeOffsetResult.Count; ++i)
        {
            int offset = ComboManager.JudgeOffsetResult[i];

            if(offset > max) 
            {
                heights[bars.Length - 1]++; 
            }
            else if(offset > curOffset + range)
            {
                --i;
                curIdx++;
                curOffset += range;
            }
            else
            {
                heights[curIdx]++;
            }

            if(heights[curIdx] > maxHeight) 
                maxHeight = heights[curIdx];
        }

        for(int i = 0; i < bars.Length; ++i) 
        {
            var size = bars[i].rectTransform.sizeDelta;
            size.y = heights[i] / maxHeight * 500;

            bars[i].rectTransform.sizeDelta = size;
        }
    }
}
