using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TapNote : NoteBase
{
    public override void ResetNote(GameNoteData data)
    {
        base.ResetNote(data);

        noteMesh.meshRenderer.sharedMaterial.SetTexture("_MainTex",
            resourceLoader.LoadSkinResource<Texture2D>("note_single_tint"));
    }
}
