﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BGEditor
{
    public class EditorSlideNote : EditorNoteBase
    {
        public Image noteImg;
        public EditorSlideBody bodyImg;

        public Sprite Long;
        public Sprite Flick;
        public Sprite SlideTick;

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

        protected override bool IsOutOfBound()
        {
            var pos = transform.localPosition;
            if (pos.y > maxHeight)
                return true;
            if (pos.y >= 0)
                return false;
            if (next == null)
                return true;
            return next.transform.localPosition.y < 0;
        }

        public override void Init(Note note)
        {
            base.Init(note);
            Debug.Assert(prev == null);
            Debug.Assert(next == null);
            SelectSlide();
            Refresh();
        }

        public override void Select()
        {
            isSelected = true;
            noteImg.color = Color.gray;
            bodyImg.SetColor(new Color(0.5843137f, 0.9019607f, 0.3019607f, 0.4f));
        }

        public override void Unselect()
        {
            Debug.Assert(next == null || !next.isSelected);
            isSelected = false;
            noteImg.color = Color.white;
            bodyImg.SetColor(new Color(0.5843137f, 0.9019607f, 0.3019607f, 0.8f));
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
                UnselectSlide();
                next.prev = null;
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
                Notes.UnselectAll();
                nxt.prev = this;
                next = nxt;
                SelectSlide();
            }
            Core.onNoteModified.Invoke(note);
            return true;
        }

        public override void Refresh()
        {
            if (IsOutOfBound())
            {
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
                return;
            }
            else
            {
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
            }
            switch (note.type)
            {
                case NoteType.Single:
                case NoteType.SlideTickEnd:
                    noteImg.sprite = Long;
                    break;
                case NoteType.Flick:
                    noteImg.sprite = Flick;
                    break;
                case NoteType.SlideTick:
                    noteImg.sprite = SlideTick;
                    break;
                default:
                    throw new NotImplementedException("Unsupported slide note type: " + note.type);
            }
            SetBodyMesh();
        }

        public override bool Remove()
        {
            if (isSelected)
            {
                var cmds = new CmdGroup();
                var i = this;
                while (i.next != null)
                    i = i.next;
                while (i != null)
                {
                    Debug.Assert(i.isSelected);
                    if (i.prev != null)
                    {
                        cmds.Add(new DisconnectNoteCmd(i.prev.note, i.note, Notes));
                    }
                    cmds.Add(new RemoveNoteCmd(i.note));
                    i = i.prev;
                }
                bool result = Core.Commit(cmds);
                Debug.Assert(result);
                return result;
            }
            if (prev == null && next == null)
            {
                return Core.Commit(new RemoveNoteCmd(note));
            }
            var cmd = new CmdGroup();
            if (prev == null)
            {
                cmd.Add(new DisconnectNoteCmd(note, next.note, Notes));
                cmd.Add(new RemoveNoteCmd(note));
                if (next.next == null)
                    cmd.Add(new RemoveNoteCmd(next.note));
            }
            else if (next == null)
            {
                cmd.Add(new DisconnectNoteCmd(prev.note, note, Notes));
                cmd.Add(new RemoveNoteCmd(note));
                if (prev.prev == null)
                    cmd.Add(new RemoveNoteCmd(prev.note));
            }
            else
            {
                cmd.Add(new DisconnectNoteCmd(note, next.note, Notes));
                if (next.next == null)
                    cmd.Add(new RemoveNoteCmd(next.note));
            }
            return Core.Commit(cmd);
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
            else if (next == null)
            {
                if (Editor.tool == EditorTool.Single)
                {
                    if (note.type != NoteType.SlideTickEnd)
                    {
                        Core.Commit(new ChangeNoteTypeCmd(note, NoteType.SlideTickEnd));
                        return;
                    }
                }
                else if (Editor.tool == EditorTool.Flick)
                {
                    if (note.type != NoteType.Flick)
                    {
                        Core.Commit(new ChangeNoteTypeCmd(note, NoteType.Flick));
                        return;
                    }
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