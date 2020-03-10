﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class NoteStyleToggleGroup : ToggleGroup
{
    protected Toggle[] persistentTogs;

    protected override void Start()
    {
        base.Start();
        persistentTogs = GetComponentsInChildren<Toggle>();
    }

    public NoteStyle GetStyle()
    {
        string name = persistentTogs.First(x => x.isOn).name;
        return (NoteStyle)Enum.Parse(typeof(NoteStyle), name, true);
    }

    public void SetStyle(NoteStyle style)
    {
        var tog = persistentTogs.First(x => x.name == Enum.GetName(typeof(NoteStyle), style));
        tog.isOn = true;
    }
}
