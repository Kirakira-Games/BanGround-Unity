using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradeColorChange : MonoBehaviour
{
    public Color[] Colors;

    public readonly string[] Texts = { "SSS", "SS", "S", "A", "B", "C", "D", "F" };
    public readonly float[] GradeDiff = { 0.998f, 0.99f, 0.97f, 0.94f, 0.90f, 0.85f, 0.6f, 0f };
    
    Slider sld;
    Image fill;
    Text txt;
    Text scoreTxt;
    double score;
    double displayScore;

    private void Start()
    {
        score = 0;
        displayScore = 0;
        sld = GetComponentInChildren<Slider>();
        fill = GameObject.Find("GradeFill").GetComponent<Image>();
        txt = GameObject.Find("GradeText").GetComponent<Text>();
        scoreTxt = GameObject.Find("ScoreText").GetComponent<Text>();
    }

    public void SetScore(double _score, float _acc)
    {
        score = _score;
        sld.value = _acc;
        for (int i = 0; i < GradeDiff.Length; i++)
        {
            if (_acc >= GradeDiff[i])
            {
                fill.color = Colors[i];
                txt.text = Texts[i];
                break;
            }
        }
    }

    void ScoreAddAnimation() {
        if (displayScore < score)
            displayScore += (score - displayScore) * 0.5;
        scoreTxt.text = string.Format("{0:0000000}", displayScore * 1000000);
    }

    private void Update()
    {
        ScoreAddAnimation();
    }
}
