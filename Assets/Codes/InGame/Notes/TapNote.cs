﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : NoteBase
{
    public override void ResetNote(GameNoteData data)
    {
        base.ResetNote(data);

        noteMesh.meshRenderer.sharedMaterial.SetTexture("_BaseMap",
            NoteUtility.LoadResource<Texture2D>("note_single_tint"));
    }
}
