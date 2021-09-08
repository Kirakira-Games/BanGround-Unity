using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using System.Globalization;

public class ColorPicker : MonoBehaviour
{
    public GameObject Blocker;
    public Slider R;
    public Slider G;
    public Slider B;
    public Slider A;
    public InputField Rvalue;
    public InputField Gvalue;
    public InputField Bvalue;
    public InputField Avalue;
    public InputField HexValue;
    public Image Preview;
    [HideInInspector]
    public Color Initial;
    [HideInInspector]
    public bool IsShowing;
    public Color color => new Color(R.value, G.value, B.value, A.value);
    public float Red
    {
        get => R.value;
        set
        {
            Rvalue.SetTextWithoutNotify(Mathf.RoundToInt(value * 255).ToString());
            R.SetValueWithoutNotify(value);
        }
    }

    public float Green
    {
        get => G.value;
        set
        {
            Gvalue.SetTextWithoutNotify(Mathf.RoundToInt(value * 255).ToString());
            G.SetValueWithoutNotify(value);
        }
    }

    public float Blue
    {
        get => B.value;
        set
        {
            Bvalue.SetTextWithoutNotify(Mathf.RoundToInt(value * 255).ToString());
            B.SetValueWithoutNotify(value);
        }
    }

    public float Alpha
    {
        get => A.value;
        set
        {
            Avalue.SetTextWithoutNotify(Mathf.RoundToInt(value * 255).ToString());
            A.SetValueWithoutNotify(value);
        }
    }

    private void UpdatePreview(float _)
    {
        Red = R.value;
        Green = G.value;
        Blue = B.value;
        Alpha = A.value;
        Preview.color = color;
        UpdateHex();
    }

    private void UpdateInputPreview(string _)
    {
        Red = float.Parse(Rvalue.text) / 255;
        Green = float.Parse(Gvalue.text) / 255;
        Blue = float.Parse(Bvalue.text) / 255;
        Alpha = float.Parse(Avalue.text) / 255;

        Preview.color = color;
        UpdateHex();
    }

    private void ParseHexPreview(string _)
    {
        var eight = new Regex("^#([A-Fa-f0-9]{2})([A-Fa-f0-9]{2})([A-Fa-f0-9]{2})([A-Fa-f0-9]{2})$");
        var six = new Regex("^#([A-Fa-f0-9]{2})([A-Fa-f0-9]{2})([A-Fa-f0-9]{2})$");
        var four = new Regex("^#([A-Fa-f0-9])([A-Fa-f0-9])([A-Fa-f0-9])([A-Fa-f0-9])$");
        var three = new Regex("^#([A-Fa-f0-9])([A-Fa-f0-9])([A-Fa-f0-9])$");

        Match match = three.Match(HexValue.text);
        int digit = 3;

        if (!match.Success)
        {
            match = four.Match(HexValue.text);
            digit = 4;
        }

        if (!match.Success)
        {
            match = six.Match(HexValue.text);
            digit = 6;
        }

        if (!match.Success)
        {
            match = eight.Match(HexValue.text);
            digit = 8;
        }

        if (!match.Success)
        {
            return;
        }

        float r = Convert.ToInt32(match.Groups[1].Value, 16);
        float g = Convert.ToInt32(match.Groups[2].Value, 16);
        float b = Convert.ToInt32(match.Groups[3].Value, 16);
        float a = digit % 4 == 0 ? Convert.ToInt32(match.Groups[4].Value, 16) : 255;

        if (digit < 6)
        {
            r = r * 16 + r;
            g = g * 16 + g;
            b = b * 16 + b;

            if (digit == 4)
                a = a * 16 + a;
        }

        Red = r / 255;
        Green = g / 255;
        Blue = b / 255;
        Alpha = a / 255;
        Preview.color = color;
    }

    private void UpdateHex()
    {
        HexValue.SetTextWithoutNotify(
            $"#{Mathf.RoundToInt(Red * 255):x}{Mathf.RoundToInt(Green * 255):x}{Mathf.RoundToInt(Blue * 255):x}{Mathf.RoundToInt(Alpha * 255):x}"
        );
    }

    public async UniTask<Color> Show()
    {
        if (IsShowing)
            return Initial;

        IsShowing = true;

        Red = Initial.r;
        Green = Initial.g;
        Blue = Initial.b;
        Alpha = Initial.a;

        gameObject.SetActive(true);
        Blocker.SetActive(true);

        UpdatePreview(0f);

        await UniTask.WaitUntil(() => !IsShowing);

        return Initial;
    }

    public void Exit(bool save)
    {
        if (save)
            Initial = color;

        gameObject.SetActive(false);
        Blocker.SetActive(false);

        IsShowing = false;
    }

    public void Awake()
    {
        R.onValueChanged.AddListener(UpdatePreview);
        G.onValueChanged.AddListener(UpdatePreview);
        B.onValueChanged.AddListener(UpdatePreview);
        A.onValueChanged.AddListener(UpdatePreview);

        Rvalue.onValueChanged.AddListener(UpdateInputPreview);
        Gvalue.onValueChanged.AddListener(UpdateInputPreview);
        Bvalue.onValueChanged.AddListener(UpdateInputPreview);
        Avalue.onValueChanged.AddListener(UpdateInputPreview);

        HexValue.onValueChanged.AddListener(ParseHexPreview);
    }
}
