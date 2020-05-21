using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Un4seen.Bass.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BGEditor
{
    public class EditorSlideNote : EditorNoteBase
    {
        public Image noteImg;
        public Image tickImg;
        public EditorSlideBody bodyImg;
        public EditorSlideNote prev { get; private set; }
        public EditorSlideNote next { get; private set; }

        private void SetBodyMesh()
        {
            if (next == null)
            {
                bodyImg.enabled = false;
                return;
            }
            bodyImg.enabled = true;
            var dir = next.transform.localPosition - transform.localPosition;
            bodyImg.SetDirection(dir);
        }

        public override void Init(Note note)
        {
            base.Init(note);
            SelectSlide();
            Refresh();
        }

        public override void Select()
        {
            isSelected = true;
            noteImg.color = Color.gray;
            bodyImg.SetColor(new Color(0.5843137f, 0.9019607f, 0.3019607f, 0.4f));
            tickImg.color = Color.gray;
        }

        public override void Unselect()
        {
            Debug.Assert(next == null || !next.isSelected);
            isSelected = false;
            noteImg.color = Color.white;
            bodyImg.SetColor(new Color(0.5843137f, 0.9019607f, 0.3019607f, 0.8f));
            tickImg.color = Color.white;
            prev?.Unselect();
        }

        public void SelectSlide()
        {
            var i = this;
            while (i.prev != null)
                i = i.prev;
            while (i.next != null)
            {
                i.Select();
                i = i.next;
            }
            Notes.SelectNote(i);
        }

        public void UnselectSlide()
        {
            var i = this;
            while (i.next != null)
                i = i.next;
            Notes.UnselectNote(i);
        }

        public void SetTickstack(int id)
        {
            Debug.Assert(prev == null || prev.note.tickStack == id); // Can only be called from head
            note.tickStack = id;
            if (next != null)
                next.SetTickstack(id);
        }

        public bool SetNext(EditorSlideNote nxt)
        {
            if (nxt == null)
            {
                if (next == null)
                    return false;
                next.prev = null;
                next.SetTickstack(Notes.slideIdPool.RegisterNext());
                UnselectSlide();
                Core.onNoteModified.Invoke(next.note);
                next = null;
            }
            else if (next != null || nxt.prev != null)
            {
                Debug.Log("Cannot set next of a slide note while they're not the tail / head.");
                return false;
            }
            else
            {
                float beat = ChartUtility.BeatToFloat(note.beat);
                float nextbeat = ChartUtility.BeatToFloat(nxt.note.beat);
                if (beat >= nextbeat - NoteUtility.EPS)
                    return false;
                UnselectSlide();
                nxt.prev = this;
                next = nxt;
                next.SetTickstack(note.tickStack);
                SelectSlide();
                Core.onNoteModified.Invoke(nxt.note);
            }
            Core.onNoteModified.Invoke(note);
            return true;
        }

        public override void Refresh()
        {
            if (prev == null || next == null)
            {
                noteImg.enabled = true;
                tickImg.enabled = false;
            }
            else
            {
                noteImg.enabled = false;
                tickImg.enabled = true;
            }
            if (prev == null)
                note.type = NoteType.Single;
            else if (next == null)
                note.type = NoteType.SlideTickEnd;
            else
                note.type = NoteType.SlideTick;
            SetBodyMesh();
        }

        public override bool Remove()
        {
            if (isSelected)
            {
                var cmds = new CmdGroup();
                for (var i = this; i != null; i = i.prev)
                {
                    Debug.Assert(i.isSelected);
                    if (i.prev != null)
                    {
                        cmds.Add(new ConnectNoteCmd(i.prev.note, null));
                    }
                    cmds.Add(new RemoveNoteCmd(note));
                }
                bool result = cmds.Commit(Core);
                return result;
            }
            if (prev == null && next == null)
            {
                return Core.Commit(new RemoveNoteCmd(note));
            }
            else if (prev == null)
            {
                var cmd = new CmdGroup();
                cmd.Add(new ConnectNoteCmd(note, null));
                cmd.Add(new RemoveNoteCmd(note));
                return Core.Commit(cmd);
            }
            else if (next == null)
            {
                var cmd = new CmdGroup();
                cmd.Add(new ConnectNoteCmd(prev.note, null));
                cmd.Add(new RemoveNoteCmd(note));
                return Core.Commit(cmd);
            }
            else
            {
                return Core.Commit(new ConnectNoteCmd(note, null));
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right || Editor.tool == EditorTool.Delete)
            {
                Remove();
                Notes.UnselectAll();
                return;
            }
            if (Editor.tool == EditorTool.Select)
            {
                if (isSelected)
                    UnselectSlide();
                else
                    SelectSlide();
                return;
            }
            if (Editor.tool == EditorTool.Slide)
            {
                var prev = Notes.singleSlideSelected;
                if (prev != null)
                {
                    Core.Commit(new ConnectNoteCmd(prev.note, note));
                    return;
                }
            }
            if (isSelected)
            {
                UnselectSlide();
                return;
            }
            Notes.UnselectAll();
            SelectSlide();
        }
    }
}
