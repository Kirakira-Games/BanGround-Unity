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
    private IModManager modManager;

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
        float modScoreMultiplier = 1.0f;

        foreach (var mod in modManager.attachedMods)
            modScoreMultiplier *= mod.ScoreMultiplier;

        score = _score * modScoreMultiplier;
        sld.value = (float)_score;
        txt.text = modManager.isAutoplay ? "AUTO": string.Format("{0:P2}", Mathf.FloorToInt((float)_acc * 10000) / 10000f);
        
    }

    void ScoreAddAnimation()
    {
        if (displayScore < score)
            displayScore += (score - displayScore) * 0.5;
        scoreTxt.text = ComboManager.GetDisplayScore(displayScore);
        fill.color = startColor * (float)(1 - displayScore) + endColor * (float)displayScore;
    }

    private void Update()
    {
        ScoreAddAnimation();
    }
}
