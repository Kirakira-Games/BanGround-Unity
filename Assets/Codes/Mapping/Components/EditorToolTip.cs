using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using BGEditor;
using System;

public class EditorToolTip : MonoBehaviour
{
    public Text text;

    private EditorNoteBase note;
    private float y;

    public static EditorToolTip Create(Transform parent)
    {
        var obj = Instantiate(Resources.Load("Prefab/Mapping/Tooltip")) as GameObject;
        obj.SetActive(false);
        obj.transform.SetParent(parent, false);
        return obj.GetComponent<EditorToolTip>();
    }

    private void UpdateText()
    {
        if (float.IsNaN(y))
        {
            text.text = "Y: Ground";
        }
        else
        {
            text.text = string.Format("Y: {0:0.000}", y);
        }
    }

    public void Show(EditorNoteBase note)
    {
        if (this.note != null) return;
        this.note = note;
        y = note.note.yOrNaN;
        transform.SetParent(note.transform, false);
        UpdateText();
        gameObject.SetActive(true);
    }

    public void Hide(EditorNoteBase note)
    {
        if (ReferenceEquals(note, this.note))
        {
            this.note = null;
            gameObject.SetActive(false);
        }
    }

    public void UpdateY(float noteY)
    {
        if (y != noteY)
        {
            y = noteY;
            UpdateText();
        }
    }
}
