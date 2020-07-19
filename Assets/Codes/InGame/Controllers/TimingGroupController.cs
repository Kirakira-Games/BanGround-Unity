using UnityEngine;
using System.Collections;
using V2;

public class TimingGroupController
{
    public TimingPoint Current;

    private GameTimingGroup group;
    private int ptr = 0;
    private MaterialPropertyBlock[] properties;
    static KVarRef r_brightness_long = new KVarRef("r_brightness_long");

    public TimingGroupController(GameTimingGroup group)
    {
        this.group = group;
        properties = new MaterialPropertyBlock[5];
        for (int i = 0; i < properties.Length; i++)
        {
            properties[i] = new MaterialPropertyBlock();
        }
    }

    public void SetColor(TimingPoint p)
    {
        // TODO: GEEKiDoS
        properties[0].SetColor("_BaseColor", p.tap);
        // TODO: GEEKiDoS
        properties[1].SetColor("_BaseColor", p.tapGrey);
        // TODO: GEEKiDoS
        properties[2].SetColor("_BaseColor", p.flick);
        // TODO: GEEKiDoS
        properties[3].SetColor("_BaseColor", p.slideTick);
        Color slide = p.slide;
        slide.a = r_brightness_long;
        // TODO: GEEKiDoS
        properties[4].SetColor("_BaseColor", slide);
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

    public MaterialPropertyBlock GetMaterialPropertyBlock(GameNoteType type, bool isGrey)
    {
        switch (type)
        {
            case GameNoteType.Single:
                return isGrey ? properties[1] : properties[0];
            case GameNoteType.Flick:
            case GameNoteType.SlideEndFlick:
                return properties[2];
            case GameNoteType.SlideStart:
            case GameNoteType.SlideTick:
            case GameNoteType.SlideEnd:
                return properties[3];
            default:
                return properties[4];
        }
    }
}
