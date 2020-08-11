using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GradeColorChange : MonoBehaviour
{
    public Color startColor;
    public Color endColor;

    [Inject]
    private ILiveSetting liveSetting;
    
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

    static KVarRef mod_autoplay = new KVarRef("mod_autoplay");

    public void SetScore(double _score, double _acc)
    {
        float modScoreMultiplier = 1.0f;

        foreach (var mod in liveSetting.attachedMods)
            modScoreMultiplier *= mod.ScoreMultiplier;

        score = _score * modScoreMultiplier;
        sld.value = (float)_score;
        txt.text = mod_autoplay ? "AUTO": string.Format("{0:P2}", Mathf.FloorToInt((float)_acc * 10000) / 10000f);
        
    }

    void ScoreAddAnimation()
    {
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
