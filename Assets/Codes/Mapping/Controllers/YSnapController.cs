using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Zenject;
using System.Linq;

namespace BGEditor
{
    public class YSnapController : MonoBehaviour
    {
        public int[] Ys;
        public Slider YPosSlider;
        public Text YPosText;
        public Toggle YFilter;

        private Dropdown dropdown;
        [Inject]
        private IEditorInfo Editor;
        [Inject]
        private IChartCore Core;
        [Inject]
        private IEditNoteController Notes;

        private void HandleSnapChange(int index)
        {
            int target = index == 0 ? 0 : Ys[index - 1];
            if (Editor.yDivision == target)
                return;
            Core.Commit(new ChangeYSnapCmd(Editor, target));
        }

        private void HandleYChange(float newY)
        {
            if (Editor.yDivision == 0)
                return;
            float target = newY / Editor.yDivision;
            if (NoteUtility.Approximately(Editor.yPos, target))
                return;
            Core.Commit(new ChangeYLayerCmd(Notes, Editor, target));
        }

        private void RefreshText()
        {
            // Refresh slider
            if (!NoteUtility.Approximately(Editor.yPos, YPosSlider.value / Editor.yDivision))
            {
                YPosSlider.SetValueWithoutNotify(Editor.yPos * Editor.yDivision);
            }
            // Refresh text
            if (Editor.yDivision == 0)
            {
                YPosText.text = "Y: Ground";
                return;
            }
            int y = Mathf.RoundToInt(YPosSlider.value);
            YPosText.text = $"Y: {y} / {Editor.yDivision}";
        }

        private void RefreshSnap(int prev, int div)
        {
            // Refresh dropdown
            if (div == 0)
            {
                dropdown.SetValueWithoutNotify(0);
            }
            else
            {
                for (int i = 0; i < Ys.Length; i++)
                {
                    if (Ys[i] == div)
                    {
                        dropdown.SetValueWithoutNotify(i + 1);
                        break;
                    }
                }
            }

            // Find nearest
            if (div == 0)
            {
                YPosSlider.value = 0;
                YPosSlider.maxValue = 0;
                RefreshText();
                return;
            }
            int mini = 0;
            for (int i = 1; i <= div; i++)
            {
                if (Mathf.Abs((float)i / div - Editor.yPos)
                    < Mathf.Abs((float)mini / div - Editor.yPos))
                {
                    mini = i;
                }
            }
            YPosSlider.SetValueWithoutNotify(0);
            YPosSlider.maxValue = div;
            YPosSlider.value = mini;
            RefreshText();
        }

        private void HandleToggle(bool isOn)
        {
            Editor.yFilter = isOn;
            Core.onYFilterSwitched.Invoke();
        }

        public void SwitchToggle()
        {
            YFilter.isOn = !YFilter.isOn;
        }

        void Start()
        {
            // Dropdown
            dropdown = GetComponent<Dropdown>();
            dropdown.ClearOptions();
            dropdown.options.Add(new Dropdown.OptionData("Ground"));
            foreach (int i in Ys)
            {
                dropdown.options.Add(new Dropdown.OptionData("1 / " + i));
            }
            dropdown.onValueChanged.AddListener(HandleSnapChange);

            // Slider
            YPosSlider.onValueChanged.AddListener(HandleYChange);
            YFilter.onValueChanged.AddListener(HandleToggle);
            Core.onYSnapModified.AddListener(RefreshSnap);
            Core.onYPosModified.AddListener(RefreshText);
            
            // Init
            YFilter.SetIsOnWithoutNotify(Editor.yFilter);
            RefreshSnap(Editor.yDivision, Editor.yDivision);
        }
    }
}
