using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradeColorChange : MonoBehaviour
{
    public Color SSSColor;
    public Color SSColor;
    public Color SColor;
    public Color AColor;
    public Color BColor;
    public Color CColor;
    public Color FColor;

    public float[] GradeDiff = { 0.998f, 0.99f, 0.97f, 0.94f, 0.90f, 0.85f };
    Slider sld;
    Image fill;
    Text txt;
    Text scoreTxt;
    float score;
    private void Start()
    {
        score = 0;
        sld = GameObject.Find("GradeSlider").GetComponent<Slider>();
        fill = GameObject.Find("GradeFill").GetComponent<Image>();
        txt = GameObject.Find("GradeText").GetComponent<Text>();
        scoreTxt = GameObject.Find("ScoreText").GetComponent<Text>();
    }
    public void SetScore(float _score)
    {
        var color = Color.white;
        score = _score;
        sld.value = score;
        if (score > GradeDiff[0] && score <= 1f) { color = SSSColor; txt.text = "SSS"; }
        if (score > GradeDiff[1] && score <= GradeDiff[0]) { color = SSColor; txt.text = "SS"; }
        if (score > GradeDiff[2] && score <= GradeDiff[1]) { color = SColor; txt.text = "S"; }
        if (score > GradeDiff[3] && score <= GradeDiff[2]) { color = AColor; txt.text = "A"; }
        if (score > GradeDiff[4] && score <= GradeDiff[3]) { color = BColor; txt.text = "B"; }
        if (score > GradeDiff[5] && score <= GradeDiff[4]) { color = CColor; txt.text = "C"; }
        if (score <= GradeDiff[5])
        {
            color = FColor; txt.text = "F";
        }

        fill.color=color;
    }
    float displayScore;
    void ScoreAddAnimation() {
        if (displayScore < score)
            displayScore += (score-displayScore)*0.5f;
        scoreTxt.text = string.Format("{0:0000000}" ,displayScore*1000000);
    }

    private void Update()
    {
        ScoreAddAnimation();
    }
}
