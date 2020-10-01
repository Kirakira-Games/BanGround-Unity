using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class ColorPicker : MonoBehaviour
{
    public GameObject Blocker;
    public Slider R;
    public Slider G;
    public Slider B;
    public Slider A;
    public Image Preview;
    [HideInInspector]
    public Color Initial;
    [HideInInspector]
    public bool IsShowing;
    public Color color => new Color(R.value, G.value, B.value, A.value);

    private void UpdatePreview(float _)
    {
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
