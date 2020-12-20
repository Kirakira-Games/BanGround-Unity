using BanGround.Game.Mods;
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

    private Slider sld;
    private Image fill;
    private Text txt;
    private Text scoreTxt;
    private double score;
    private double displayScore;

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
        score = _score * modManager.ScoreMultiplier;
        sld.value = (float)_score;
        txt.text = modManager.Flag.HasFlag(ModFlag.AutoPlay) ? "AUTO": string.Format("{0:P2}", Mathf.FloorToInt((float)_acc * 10000) / 10000f);
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
