using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BGEditor
{
    public class EditorSlideNote : EditorNoteBase
    {
        public EditorSlideBody bodyImg;

        public Sprite Long;
        public Sprite Flick;
        public Sprite SlideTick;
        public Color slideColor;

        public EditorSlideNote prev { get; private set; }
        public EditorSlideNote next { get; private set; }

        public void UpdateBodyMesh()
        {
            if (next == null || !gameObject.activeSelf)
            {
                bodyImg.enabled = false;
                bodyImg.polyCollider.enabled = false;
                return;
            }
            bodyImg.polyCollider.enabled = isActiveThisFrame;
            bodyImg.enabled = true;
            bodyImg.SetDirection(next.transform.localPosition - transform.localPosition);
            if (isSelected)
                bodyImg.SetColor(1);
            else if (!isActiveThisFrame)
                bodyImg.SetColor(2);
            else
                bodyImg.SetColor(0);
        }

        protected override bool IsOutOfBound()
        {
            if (!IsOutOfBound(this))
                return false;
            if (prev != null && prev.note.beat[0] <= Grid.EndBar && note.beat[0] >= Grid.StartBar)
                return false;
            if (next != null && next.note.beat[0] >= Grid.StartBar && note.beat[0] <= Grid.EndBar)
                return false;
            return true;
        }

        public override void Init(V2.Note note)
        {
            base.Init(note);
            Debug.Assert(prev == null);
            Debug.Assert(next == null);
            SelectSlide();
        }

        public override void Select()
        {
            base.Select();
        }

        public override void Unselect()
        {
            Debug.Assert(next == null || !next.isSelected);
            base.Unselect();
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
            base.Refresh();
            if (!gameObject.activeSelf)
                return;
            switch (note.type)
            {
                case NoteType.Single:
                case NoteType.SlideTickEnd:
                    image.sprite = Long;
                    break;
                case NoteType.Flick:
                    image.sprite = Flick;
                    break;
                case NoteType.SlideTick:
                    image.sprite = SlideTick;
                    break;
                default:
                    throw new NotImplementedException("Unsupported slide note type: " + note.type);
            }
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

        public override void OnClick(PointerEventData eventData)
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
            if (Editor.tool == EditorTool.Slide || HotKeyManager.isCtrl)
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

        protected override void Update()
        {
            bool doRefresh = shouldRefresh;
            base.Update();
            if (doRefresh)
                UpdateBodyMesh();
        }
    }
}
