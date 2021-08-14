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
        get
        {
            return R.value;
        }
        set
        {
            Rvalue.text = Mathf.RoundToInt(value * 255).ToString();
            R.value = value;
        }
    }

    public float Green
    {
        get
        {
            return G.value;
        }
        set
        {
            Gvalue.text = Mathf.RoundToInt(value * 255).ToString();
            G.value = value;
        }
    }

    public float Blue
    {
        get
        {
            return B.value;
        }
        set
        {
            Bvalue.text = Mathf.RoundToInt(value * 255).ToString();
            B.value = value;
        }
    }

    public float Alpha
    {
        get
        {
            return A.value;
        }
        set
        {
            Avalue.text = Mathf.RoundToInt(value * 255).ToString();
            A.value = value;
        }
    }

    private void UpdatePreview(float _)
    {
        DisableListener();
        {
            Red = R.value;
            Green = G.value;
            Blue = B.value;
            Alpha = A.value;
            Preview.color = color;
            UpdateHex();
        }
        EnableListener();
    }

    private void UpdateInputPreview(string _)
    {
        DisableListener();
        {
            Red = float.Parse(Rvalue.text) / 255;
            Green = float.Parse(Gvalue.text) / 255;
            Blue = float.Parse(Bvalue.text) / 255;
            Alpha = float.Parse(Avalue.text) / 255;
            Preview.color = color;
            UpdateHex();
        }
        EnableListener();
    }

    private void ParseHexPreview(string _)
    {
        var regex = new Regex("^#(\\w{1,2})(\\w{1,2})(\\w{1,2})(\\w{1,2})?$", RegexOptions.RightToLeft);

        var m = regex.Match(HexValue.text);

        Debug.Log(m.Groups.Count);

        if(m.Success)
        {
            float r = Convert.ToInt32(m.Groups[1].Value, 16);
            float g = Convert.ToInt32(m.Groups[2].Value, 16);
            float b = Convert.ToInt32(m.Groups[3].Value, 16);
            float a = !string.IsNullOrEmpty(m.Groups[4].Value) ? Convert.ToInt32(m.Groups[4].Value, 16) : 255;

            if (m.Groups[1].Value.Length == 1)
                r = r * 16 + r;
            if (m.Groups[2].Value.Length == 1)
                g = g * 16 + g;
            if (m.Groups[3].Value.Length == 1)
                b = b * 16 + b;
            if (m.Groups[4].Value?.Length == 1)
                a = a * 16 + a;

            DisableListener();
            {
                Red = r / 255;
                Green = g / 255;
                Blue = b / 255;
                Alpha = a / 255;
                Preview.color = color;
            }
            EnableListener();
        }
    }

    private void UpdateHex()
    {
        HexValue.text = $"#{Mathf.RoundToInt(Red * 255):x}{Mathf.RoundToInt(Green * 255):x}{Mathf.RoundToInt(Blue * 255):x}{Mathf.RoundToInt(Alpha * 255):x}";
    }

    public async UniTask<Color> Show()
    {
        if (IsShowing) return Initial;
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

    void DisableListener()
    {
        R.onValueChanged.RemoveListener(UpdatePreview);
        G.onValueChanged.RemoveListener(UpdatePreview);
        B.onValueChanged.RemoveListener(UpdatePreview);
        A.onValueChanged.RemoveListener(UpdatePreview);

        Rvalue.onValueChanged.RemoveListener(UpdateInputPreview);
        Gvalue.onValueChanged.RemoveListener(UpdateInputPreview);
        Bvalue.onValueChanged.RemoveListener(UpdateInputPreview);
        Avalue.onValueChanged.RemoveListener(UpdateInputPreview);

        HexValue.onValueChanged.RemoveListener(ParseHexPreview);
    }

    void EnableListener()
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

    public void Awake()
    {
        EnableListener();
    }

}
