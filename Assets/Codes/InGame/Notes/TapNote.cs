using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : NoteBase
{
    protected override void Start()
    {
        base.Start();
        sprite.sprite = Resources.Load<Sprite>("V2Assets/note_single_default");
    }
}
