using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatInput : MonoBehaviour
{
    public InputField Bar;
    public InputField Numerator;
    public InputField Denominator;
    public int[] beat { get; private set; }

    public Color NormalColor;
    public Color ErrorColor;

    private int[] mBeat;

    private void Validate(string _)
    {
        var inputs = new InputField[] { Bar, Numerator, Denominator };
        bool success = true;
        for (int i = 0; i < inputs.Length; i++)
        {
            InputField field = inputs[i];
            if (!int.TryParse(field.text, out mBeat[i]) || mBeat[i] < 0 || mBeat[i] >= 10000)
            {
                success = false;
                field.textComponent.color = ErrorColor;
            }
            else
            {
                field.textComponent.color = NormalColor;
            }
        }
        if (!success)
            return;
        if (mBeat[2] == 0 || mBeat[2] > 128)
        {
            success = false;
            Denominator.textComponent.color = ErrorColor;
        }
        else
        {
            Denominator.textComponent.color = NormalColor;
        }
        if (mBeat[1] >= mBeat[2])
        {
            success = false;
            Numerator.textComponent.color = ErrorColor;
        }
        else
        {
            Numerator.textComponent.color = NormalColor;
        }
        if (!success)
            return;

        mBeat.CopyTo(beat, 0);
    }

    public void SetValue(int[] newBeat)
    {
        Debug.Assert(newBeat.Length == 3);
        Bar.text = newBeat[0].ToString();
        Numerator.text = newBeat[1].ToString();
        Denominator.text = newBeat[2].ToString();
    }

    void Awake()
    {
        beat = new int[3];
        mBeat = new int[3];
        Bar.onValueChanged.AddListener(Validate);
        Numerator.onValueChanged.AddListener(Validate);
        Denominator.onValueChanged.AddListener(Validate);
        Bar.text = "0";
        Numerator.text = "0";
        Denominator.text = "1";
    }
}
