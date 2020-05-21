using System;
using System.Collections.Generic;
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
        public Image bodyImg;
        public Image tickImg;
        public EditorSlideNote prev { get; private set; }
        public EditorSlideNote next { get; private set; }

        private CanvasRenderer bodyRenderer;
        private Vector3 widthDelta;
        private static readonly Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };
        private static readonly int[] triangles = new int[]
        {
            1, 0, 3, 3, 0, 2
        };
        private Mesh mesh;

        private void SetBodyMesh()
        {
            if (next == null)
            {
                bodyImg.enabled = false;
                return;
            }
            bodyImg.enabled = true;
            var dir = next.transform.localPosition - transform.localPosition;
            var vertices = new Vector3[]
            {
                -widthDelta,
                widthDelta,
                dir - widthDelta,
                dir + widthDelta
            };
            mesh.SetVertices(vertices);
            bodyRenderer.SetMesh(mesh);
        }

        public override void Init(Note note)
        {
            base.Init(note);
            Refresh();
        }

        public override void Select()
        {
            isSelected = true;
            noteImg.color = Color.gray;
            bodyImg.color = Color.gray;
            tickImg.color = Color.gray;
        }

        public override void Unselect()
        {
            isSelected = false;
            noteImg.color = Color.white;
            bodyImg.color = Color.white;
            tickImg.color = Color.white;
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
            while (i.prev != null)
                i = i.prev;
            while (i.next != null)
            {
                i.Unselect();
                i = i.next;
            }
            Notes.UnselectNote(i);
        }

        protected override void Awake()
        {
            base.Awake();
            bodyRenderer = bodyImg.GetComponent<CanvasRenderer>();
            widthDelta = Vector3.right * GetComponent<RectTransform>().rect.width * 0.45f;
            mesh = new Mesh();
            mesh.vertices = new Vector3[4];
            mesh.uv = uvs;
            mesh.triangles = triangles;
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
                Core.onNoteModified.Invoke(next.note);
                next = null;
            }
            else if (next != null || nxt.prev != null)
            {
                Debug.LogWarning("Cannot set next of a slide note while they're not the tail / head.");
                return false;
            }
            else
            {
                float beat = ChartUtility.BeatToFloat(note.beat);
                float nextbeat = ChartUtility.BeatToFloat(nxt.note.beat);
                if (beat >= nextbeat - NoteUtility.EPS)
                    return false;
                nxt.prev = this;
                next = nxt;
                next.SetTickstack(note.tickStack);
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

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right || Editor.tool == EditorTool.Delete)
            {
                if (prev == null && next == null)
                {
                    Core.Commit(new RemoveNoteCmd(note));
                }
                else if (prev == null)
                {
                    var cmd = new CmdGroup();
                    cmd.Add(new ConnectNoteCmd(note, null));
                    cmd.Add(new RemoveNoteCmd(note));
                    Core.Commit(cmd);
                }
                else if (next == null)
                {
                    var cmd = new CmdGroup();
                    cmd.Add(new ConnectNoteCmd(prev.note, null));
                    cmd.Add(new RemoveNoteCmd(note));
                    Core.Commit(cmd);
                }
                else
                {
                    Core.Commit(new ConnectNoteCmd(note, null));
                }
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
