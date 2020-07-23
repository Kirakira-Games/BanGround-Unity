using UnityEngine;
using System.Collections;
using V2;

public class FlickArrow : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.MainModule main;
    private ParticleSystemRenderer psRenderer;

    private void Awake()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        psRenderer = GetComponentInChildren<ParticleSystemRenderer>();
        main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
    }

    public void Reset(TimingGroupController timingGroup)
    {
        psRenderer.sharedMaterial = timingGroup.GetMaterial(GameNoteType.Flick, psRenderer.sharedMaterial, true);
    }
}
