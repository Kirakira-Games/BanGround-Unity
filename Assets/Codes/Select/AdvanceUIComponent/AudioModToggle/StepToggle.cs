using BanGround.Game.Mods;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StepToggle : MonoBehaviour,IPointerClickHandler
{
    public StepInfo[] Steps;
    public StepToggle ConflictTog;
    public Text Txt;
    public Image Img;
    protected int index = 0;

    [HideInInspector] public AudioMod currentMod;

    public void OnPointerClick(PointerEventData eventData)
    {
        index++;
        index %= Steps.Length;
        OnIndexChanged();
        if (index != 0) ConflictTog.SetStep(ModFlag.None);
    }

    public virtual ModFlag GetStep() { return ModFlag.None; }

    public virtual void SetStep(ModFlag mods) { }

    protected void OnIndexChanged()
    {
        Txt.text = Steps[index].StepText;
        Img.sprite = Steps[index].StepImg;
    }
}

[System.Serializable]
public class StepInfo
{
    public string StepText;
    public Sprite StepImg;
}
