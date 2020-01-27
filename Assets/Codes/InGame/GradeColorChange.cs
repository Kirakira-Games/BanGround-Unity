using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradeColorChange : MonoBehaviour
{
    public Color startColor;
    public Color endColor;
    
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

    public void SetScore(double _score, double _acc)
    {
        score = _score;
        sld.value = (float)_score;
        txt.text = LiveSetting.autoPlayEnabled ? "AUTO": string.Format("{0:P2}", _acc);
        
    }

    void ScoreAddAnimation() {
        if (displayScore < score)
            displayScore += (score - displayScore) * 0.5;
        scoreTxt.text = string.Format("{0:0000000}", displayScore * 1000000);
        fill.color = startColor * (float)(1 - displayScore) + endColor * (float)displayScore;
    }

    private void Update()
    {
        ScoreAddAnimation();
    }
}
