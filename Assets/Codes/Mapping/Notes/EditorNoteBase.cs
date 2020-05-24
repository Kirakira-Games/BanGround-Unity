using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BGEditor
{
    public abstract class EditorNoteBase : CoreMonoBehaviour, IPointerClickHandler
    {
        public Note note { get; private set; }

        protected Image image;
        public bool isSelected { get; protected set; }

        protected float maxHeight;

        public virtual void Select()
        {
            isSelected = true;
            image.color = Color.gray;
        }

        public virtual void Unselect()
        {
            isSelected = false;
            image.color = Color.white;
        }

        public virtual bool Remove()
        {
            return Core.Commit(new RemoveNoteCmd(note));
        }

        protected void Awake()
        {
            image = GetComponent<Image>();
        }

        public virtual void Init(Note note)
        {
            this.note = note;
            Unselect();
            transform.SetParent(Grid.transform, false);
            maxHeight = Grid.GetComponent<RectTransform>().rect.height;
            UpdatePosition();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
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
                    Notes.UnselectNote(this);
                else
                    Notes.SelectNote(this);
                return;
            }
            if (isSelected)
            {
                Notes.UnselectNote(this);
                return;
            }
            Notes.UnselectAll();
            Notes.SelectNote(this);
        }

        public virtual void UpdatePosition()
        {
            var pos = Grid.GetLocalPosition(note.lane, note.beat);
            transform.localPosition = pos;
        }

        protected virtual bool IsOutOfBound()
        {
            var pos = transform.localPosition;
            if (pos.y < 0)
                return true;
            if (pos.y > maxHeight)
                return true;
            return false;
        }

        public virtual void Refresh()
        {
            if (IsOutOfBound())
            {
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
            }
            else
            {
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
            }
        }
    }
}
