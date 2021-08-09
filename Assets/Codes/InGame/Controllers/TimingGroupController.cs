using UnityEngine;
using System.Collections.Generic;
using V2;

public class TimingGroupController
{
    public TimingPoint Current;

    private GameTimingGroup group;
    private int ptr = 0;
    public Material[] materials;
    KVar r_brightness_long;

    public TimingGroupController(GameTimingGroup group, KVar r_brightness_long)
    {
        this.r_brightness_long = r_brightness_long;
        this.group = group;
        materials = new Material[7];
    }

    public Material GetMaterial(GameNoteType type, Material current, bool isSpecial = false)
    {
        int index;
        switch (type)
        {
            case GameNoteType.Flick:
            case GameNoteType.SlideEndFlick: // Normal = 0, Arrow = 1
                index = isSpecial ? 1 : 0;
                break;
            case GameNoteType.Single: // Normal = 2, Grey = 3
                index = isSpecial ? 3 : 2;
                break;
            case GameNoteType.SlideEnd:
            case GameNoteType.SlideStart:
                index = 4;
                break;
            case GameNoteType.SlideTick:
                index = 5;
                break;
            default:
                index = 6;
                break;
        }
        if (materials[index] == null)
        {
            materials[index] = new Material(current);
        }
        return materials[index];
    }

    public void SetColor(TimingPoint p)
    {
        materials[0]?.SetColor("_Tint", p.flick);

        materials[1]?.SetColor("_Tint", p.flick);

        materials[2]?.SetColor("_Tint", p.tap);

        materials[3]?.SetColor("_Tint", p.tapGrey);

        materials[4]?.SetColor("_Tint", p.slideTick);

        materials[5]?.SetColor("_Tint", p.slideTick);

        Color slide = p.slide;
        slide.a *= r_brightness_long;
        materials[6]?.SetColor("_BaseColor", slide);
    }

    public void OnUpdate()
    {
        float time = NoteController.audioTimef;
        while (ptr < group.points.Count - 1 && group.points[ptr + 1].time <= time) ptr++;
        while (ptr > 0 && group.points[ptr].time > time) ptr--;
        if (ptr == group.points.Count - 1)
        {
            Current = group.points[ptr];
        }
        else
        {
            float t = Mathf.InverseLerp(group.points[ptr].time, group.points[ptr + 1].time, time);
            Current = TimingPoint.Lerp(group.points[ptr], group.points[ptr + 1], t);
        }
        SetColor(Current);
    }
}
