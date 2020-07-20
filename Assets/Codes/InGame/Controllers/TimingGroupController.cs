using UnityEngine;
using System.Collections.Generic;
using V2;

public class TimingGroupController
{
    public TimingPoint Current;

    private GameTimingGroup group;
    private int ptr = 0;
    public Material[] materials;
    static KVarRef r_brightness_long = new KVarRef("r_brightness_long");

    public TimingGroupController(GameTimingGroup group)
    {
        this.group = group;
        materials = new Material[6];
    }

    public Material GetMaterial(GameNoteType type, Material current, bool isGrey = false)
    {
        int index;
        switch (type)
        {
            case GameNoteType.Flick:
            case GameNoteType.SlideEndFlick:
                index = 0;
                break;
            case GameNoteType.Single:
                index = isGrey ? 2 : 1;
                break;
            case GameNoteType.SlideEnd:
            case GameNoteType.SlideStart:
                index = 3;
                break;
            case GameNoteType.SlideTick:
                index = 4;
                break;
            default: // Slide body
                index = 5;
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
        // TODO: GEEKiDoS
        materials[0]?.SetColor("_BaseColor", p.flick);
        // TODO: GEEKiDoS
        materials[1]?.SetColor("_BaseColor", p.tap);
        // TODO: GEEKiDoS
        materials[2]?.SetColor("_BaseColor", p.tapGrey);
        // TODO: GEEKiDoS
        materials[3]?.SetColor("_BaseColor", p.slideTick);
        // TODO: GEEKiDoS
        materials[4]?.SetColor("_BaseColor", p.slideTick);
        Color slide = p.slide;
        slide.a = r_brightness_long;
        // TODO: GEEKiDoS
        materials[5]?.SetColor("_BaseColor", slide);
    }

    public void OnUpdate()
    {
        float time = NoteController.audioTime / 1000f;
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
