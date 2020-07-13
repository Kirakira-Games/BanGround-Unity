using UnityEngine;
using System.Collections;
using V2;

public class TimingGroupController
{
    private GameTimingGroup group;
    private Material[] materials;
    private int ptr = 0;
    static KVarRef r_brightness_long = new KVarRef("r_brightness_long");

    public TimingGroupController(GameTimingGroup group)
    {
        this.group = group;
        materials = new Material[]
        {
            // TODO: GEEKiDoS
        };
    }

    public void SetColor(TimingPoint p)
    {
        // TODO: GEEKiDoS
        materials[0].SetColor("_BaseColor", p.tap);
        // TODO: GEEKiDoS
        materials[1].SetColor("_BaseColor", p.tapGrey);
        // TODO: GEEKiDoS
        materials[2].SetColor("_BaseColor", p.flick);
        // TODO: GEEKiDoS
        materials[3].SetColor("_BaseColor", p.slideTick);
        Color slide = p.slide;
        slide.a = r_brightness_long;
        // TODO: GEEKiDoS
        materials[4].SetColor("_BaseColor", slide);
    }

    public void OnUpdate()
    {
        float time = NoteController.audioTime / 1000f;
        while (ptr < group.points.Count - 1 && group.points[ptr + 1].time <= time) ptr++;
        if (ptr == group.points.Count - 1)
        {
            SetColor(group.points[ptr]);
        }
        else
        {
            float t = Mathf.InverseLerp(group.points[ptr].time, group.points[ptr + 1].time, time);
            SetColor(TimingPoint.Lerp(group.points[ptr], group.points[ptr + 1], t));
        }
    }

    public Material GetMaterial(GameNoteType type, bool isGrey)
    {
        switch (type)
        {
            case GameNoteType.Single:
                return isGrey ? materials[1] : materials[0];
            case GameNoteType.Flick:
            case GameNoteType.SlideEndFlick:
                return materials[2];
            case GameNoteType.SlideTick:
                return materials[3];
            default:
                return materials[4];
        }
    }
}
