using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MessageCenter : MonoBehaviour, IMessageCenter
{
    public Text Title;
    public Text Word;
    public float Duration;

    private float timer = float.NaN;

    public void Show(string title, string word)
    {
        Title.text = title;
        Word.text = word;
        if (float.IsNaN(timer))
        {
            gameObject.SetActive(true);
        }
        timer = 0;
    }

    void Update()
    {
        if (!float.IsNaN(timer))
        {
            timer += Time.deltaTime;
            if (timer >= Duration)
            {
                gameObject.SetActive(false);
                timer = float.NaN;
            }
        }
    }
}