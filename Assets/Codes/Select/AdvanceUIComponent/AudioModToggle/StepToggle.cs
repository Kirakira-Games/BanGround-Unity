using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StepToggle : MonoBehaviour,IPointerClickHandler
{
    public StepInfo[] Steps;
    public StepToggle ConflictTog;

    private Text txt;
    private Image img;
    protected int index = 0;

    [HideInInspector] public AudioMod currentMod;

    private void Awake()
    {
        txt = GetComponentInChildren<Text>();
        img = GetComponentInChildren<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click");
        index++;
        index %= Steps.Length;
        OnIndexChanged();
        if (index != 0) ConflictTog.SetStep(null);
    }

    public virtual AudioMod GetStep() { return null; }

    public virtual void SetStep(List<ModBase> mods) { }

    protected void OnIndexChanged()
    {
        txt.text = Steps[index].StepText;
        img.sprite = Steps[index].StepImg;
    }
}

[System.Serializable]
public class StepInfo
{
    public string StepText;
    public Sprite StepImg;
}
