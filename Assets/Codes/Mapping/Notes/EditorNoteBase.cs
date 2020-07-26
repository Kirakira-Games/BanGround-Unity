using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BGEditor
{
    public abstract class EditorNoteBase : CoreMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public V2.Note note { get; private set; }
        public Image image;
        public Slider YSlider;

        public bool isSelected { get; protected set; }
        public bool isActive
        {
            get
            {
                if (note.group != Editor.currentTimingGroup)
                    return false;
                if (!Editor.yFilter)
                    return true;
                if (note.lane == -1)
                    return Mathf.Approximately(note.y, Editor.yPos);
                return Editor.yDivision == 0;
            }
        }

        protected float maxHeight;
        protected bool shouldRefresh;

        #region Action
        public virtual void Select()
        {
            isSelected = true;
            shouldRefresh = true;
        }

        public virtual void Unselect()
        {
            isSelected = false;
            shouldRefresh = true;
        }

        public virtual bool Remove()
        {
            return Core.Commit(new RemoveNoteCmd(note));
        }

        public virtual void Init(V2.Note note)
        {
            this.note = note;
            Unselect();
            transform.SetParent(Grid.transform, false);
            maxHeight = Grid.GetComponent<RectTransform>().rect.height;
            UpdatePosition();
        }
        #endregion

        #region Interaction
        protected float pointerDownTime = float.NaN;
        protected Vector2 holdStart = Vector2.zero;
        protected bool isHover = false;
        protected bool isAdjustY = false;
        public const float HOLD_TIME = 0.6f;
        public const float SLIDER_HEIGHT = 180;

        public virtual void OnClick(PointerEventData eventData)
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

        public void OnHold()
        {
            if (isAdjustY)
                return;
            isAdjustY = true;
            holdStart = Input.mousePosition;
            YSlider.maxValue = Editor.yDivision;
            YSlider.value = Mathf.RoundToInt(note.y / (1f / Editor.yDivision));
            YSlider.gameObject.SetActive(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                pointerDownTime = 0;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerDownTime = float.NaN;
            isAdjustY = false;
            holdStart = Vector2.zero;
            if (YSlider.gameObject.activeSelf)
            {
                float val = YSlider.value / YSlider.maxValue;
                if (!Mathf.Approximately(note.y, val))
                {
                    Core.Commit(new ChangeNoteYCmd(note, val));
                }
                YSlider.gameObject.SetActive(false);
            }
            if (isHover && (float.IsNaN(pointerDownTime) || pointerDownTime <= HOLD_TIME))
            {
                OnClick(eventData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHover = false;
        }

        public void OnMove()
        {
            if (isAdjustY)
            {
                float y = ((Vector2)Input.mousePosition - holdStart).y;
                y = y / SLIDER_HEIGHT + note.y;
                YSlider.value = Mathf.RoundToInt(Mathf.Lerp(0, YSlider.maxValue, y));
            }
            else
            {
                // TODO: handle move event
            }
        }
        #endregion

        #region View
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
            bool active = isActive;
            Refresh(active, GetColor(isSelected, active));
        }

        protected Color GetColor(bool selected, bool active)
        {
            Color c = selected ? new Color(0.5f, 1f, 1f) : Color.white;
            c.a = active ? 1f : 0.3f;
            return c;
        }

        public void UpdatePosition()
        {
            transform.localPosition = Grid.GetLocalPosition(note.lane == -1 ? note.x : note.lane, note.beat);
        }

        public virtual void Refresh(bool active, Color color)
        {
            image.raycastTarget = active;
            image.color = color;
            // Update whether display
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
        #endregion

        private void Update()
        {
            if (shouldRefresh)
            {
                Refresh();
                shouldRefresh = false;
            }
            if (!float.IsNaN(pointerDownTime))
            {
                pointerDownTime += Time.deltaTime;
                OnMove();
                if (pointerDownTime > HOLD_TIME)
                {
                    OnHold();
                }
            }
        }
    }
}
