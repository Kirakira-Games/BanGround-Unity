using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 绿条中的不是首部的节点, 后面会带一个NoteMesh
public class AfterNoteBase : NoteBase
{
    NoteMesh noteMesh;
    NoteBase frontNote;

    protected override void Start()
    {
        noteMesh = gameObject.AddComponent<NoteMesh>();
        noteMesh.frontNoteTrans = frontNote.transform;
        noteMesh.afterNoteTrans = transform;

        //noteMesh.material = 
    }

    public override void OnNoteUpdate()
    {
        base.OnNoteUpdate();
        noteMesh.OnUpdate();
    }
}
