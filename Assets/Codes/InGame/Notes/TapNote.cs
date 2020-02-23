﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : NoteBase
{
    public override void ResetNote()
    {
        base.ResetNote();
        GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", NoteUtility.LoadResource<Texture2D>(
            isGray ? "note_single_grey" : "note_single_default"));
    }
}
