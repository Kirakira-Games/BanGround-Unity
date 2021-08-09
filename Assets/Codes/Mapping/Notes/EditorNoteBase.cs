using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Zenject;

namespace BGEditor
{
    public abstract class EditorNoteBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Inject]
        protected IChartCore Core;
        [Inject]
        protected IEditNoteController Notes;
        [Inject]
        protected IGridController Grid;
        [Inject]
        protected IEditorInfo Editor;

        public V2.Note note { get; private set; }
        public Image image;
        public Slider YSlider;

        public bool isSelected { get; protected set; }
        public bool isActive
        {
            get
            {
                if (note.group != Editor.currentTimingGroup || Editor.isSpeedView)
                    return false;
                if (!Editor.yFilter)
                    return true;
                if (note.lane == -1)
                    return NoteUtility.Approximately(note.y, Editor.yPos);
                return Editor.yDivision == 0;
            }
        }

        [HideInInspector]
        public bool isActiveThisFrame;

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
            if (Editor.tool == EditorTool.Select || HotKeyManager.isCtrl)
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
            holdStart = InputSystemTouchProvider.mouseOrTouchPosition;
            if (Editor.tool == EditorTool.Select)
            {
                YSlider.wholeNumbers = false;
                YSlider.maxValue = 1;
                YSlider.value = note.y;
                YSlider.interactable = false;
            }
            else
            {
                YSlider.wholeNumbers = true;
                YSlider.maxValue = Editor.yDivision;
                YSlider.value = Mathf.RoundToInt(note.y / (1f / Editor.yDivision));
                YSlider.interactable = true;
            }
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
            if (isAdjustY)
            {
                if (YSlider.interactable)
                {
                    if (Editor.yDivision == 0 && note.lane < 0)
                    {
                        Core.Commit(new ChangeNoteYCmd(Notes, note, float.NaN));
                    }
                    else
                    {
                        float val = YSlider.value / YSlider.maxValue;
                        if (note.lane >= 0 || !NoteUtility.Approximately(note.y, val))
                        {
                            Core.Commit(new ChangeNoteYCmd(Notes, note, val));
                        }
                    }
                }
                if (!isHover)
                    Core.tooltip.Hide(this);
            }
            if (isHover && (float.IsNaN(pointerDownTime) || pointerDownTime <= HOLD_TIME))
            {
                OnClick(eventData);
            }
            isAdjustY = false;
            holdStart = Vector2.zero;
            pointerDownTime = float.NaN;
            YSlider.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHover = true;
            Core.tooltip.Show(this);
        }

        public async void OnPointerExit(PointerEventData eventData)
        {
            // 等一帧 保证OnPointerUp先触发
            await UniTask.DelayFrame(1);
            isHover = false;
        }

        public void OnMove()
        {
            if (isAdjustY)
            {
                if (YSlider.interactable)
                {
                    float y = (InputSystemTouchProvider.mouseOrTouchPosition - holdStart).y;
                    y = y / SLIDER_HEIGHT + note.y;
                    y = Mathf.RoundToInt(Mathf.Lerp(0, YSlider.maxValue, y));
                    YSlider.value = y;
                    Core.tooltip.UpdateY(Editor.yDivision == 0 ? float.NaN : y / YSlider.maxValue);
                }
            }
            else
            {
                // TODO: handle move event
            }
        }
        #endregion

        #region View
        protected bool IsStrictlyOutOfBound()
        {
            int bar = note.beat[0];
            return bar < Grid.StartBar || bar > Grid.EndBar;
        }

        protected virtual bool IsOutOfBound()
        {
            return IsStrictlyOutOfBound();
        }

        protected Color GetColor(bool selected, bool active)
        {
            Color c = selected ? new Color(0f, 1f, 1f) : Color.white;
            c.a = active ? 1f : 0.3f;
            return c;
        }

        private void UpdatePosition()
        {
            transform.localPosition = Grid.GetLocalPosition(note.lane == -1 ? note.x : note.lane, note.beat);
        }

        public virtual void Refresh()
        {
            isActiveThisFrame = isActive;
            image.raycastTarget = isActiveThisFrame;
            image.color = GetColor(isSelected, isActiveThisFrame);
            // Update whether display
            if (IsOutOfBound() || Editor.currentTimingGroup != note.group)
            {
                if (gameObject.activeSelf)
                {
                    Core.tooltip.Hide(this);
                    gameObject.SetActive(false);
                }
            }
            else
            {
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
                UpdatePosition();
            }
        }
        #endregion

        protected virtual void Update()
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
            if (!isHover && !isAdjustY)
            {
                Core.tooltip.Hide(this);
            }
        }
    }
}
