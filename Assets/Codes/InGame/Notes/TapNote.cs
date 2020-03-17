using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : NoteBase
{
    public override void ResetNote(GameNoteData data)
    {
        base.ResetNote(data);
        GetComponent<SpriteRenderer>().sprite = NoteUtility.LoadResource<Sprite>(
            isGray ? "note_single_grey" : "note_single_default");
    }
}
