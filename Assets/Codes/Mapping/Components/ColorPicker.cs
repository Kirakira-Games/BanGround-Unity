using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class ColorPicker : MonoBehaviour
{
    public GameObject Blocker;
    public Slider R;
    public Text Rvalue;
    public Slider G;
    public Text Gvalue;
    public Slider B;
    public Text Bvalue;
    public Slider A;
    public Text Avalue;
    public Image Preview;
    [HideInInspector]
    public Color Initial;
    [HideInInspector]
    public bool IsShowing;
    public Color color => new Color(R.value, G.value, B.value, A.value);
    private void UpdatePreview(float _)
    {
        Rvalue.text = (Mathf.RoundToInt(color.r * 255)).ToString();
        Gvalue.text = (Mathf.RoundToInt(color.g * 255)).ToString();
        Bvalue.text = (Mathf.RoundToInt(color.b * 255)).ToString();
        Avalue.text = (Mathf.RoundToInt(color.a * 255)).ToString();
        Preview.color = color;
    }

    public async UniTask<Color> Show()
    {
        if (IsShowing) return Initial;
        IsShowing = true;
        R.value = Initial.r;
        G.value = Initial.g;
        B.value = Initial.b;
        A.value = Initial.a;
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
    }
}
