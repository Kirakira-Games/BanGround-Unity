using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class RangeInput : MonoBehaviour
{
    public int numDigits;
    public float min;
    public float max;
    public float step;
    public bool loop;

    private float value;
    private InputField inputObject;

    void UpdateDisplay()
    {
        if (numDigits == 0)
            inputObject.text = Mathf.RoundToInt(value).ToString();
        else
            inputObject.text = string.Format("{0:f" + numDigits + "}", value);
    }

    void Start()
    {
        inputObject = GetComponent<InputField>();
        value = float.Parse(inputObject.text);
        inputObject.onValueChanged.AddListener((string text) =>
        {
            value = float.Parse(text);
        });
    }

    public void Increase()
    {
        value += step;
        if (value >= max + NoteUtility.EPS)
        {
            if (loop)
                value = min;
            else
                value = max;
        }
        UpdateDisplay();
    }

    public void Decrease()
    {
        value -= step;
        if (value <= min - NoteUtility.EPS)
        {
            if (loop)
                value = max;
            else
                value = min;
        }
        UpdateDisplay();
    }
}
