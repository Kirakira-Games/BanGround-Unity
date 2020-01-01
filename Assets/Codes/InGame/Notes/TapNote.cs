using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : NoteBase
{
    protected override void Start()
    {
        base.Start();
        sprite.sprite = NoteUtility.LoadResource<Sprite>("note_single_default");
    }
}
