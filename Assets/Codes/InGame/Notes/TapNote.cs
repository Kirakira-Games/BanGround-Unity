using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : NoteBase
{
    public override void InitNote()
    {
        base.InitNote();
        mesh.material.SetTexture("_MainTex", NoteUtility.LoadResource<Texture2D>(
            isGray ? "note_single_grey" : "note_single_default"));
    }
}
