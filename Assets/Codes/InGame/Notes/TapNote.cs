using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : NoteBase
{
    protected override void Start()
    {
        base.Start();
        mesh.material.SetTexture("_MainTex", NoteUtility.LoadResource<Texture2D>("note_single_default"));
    }
}
