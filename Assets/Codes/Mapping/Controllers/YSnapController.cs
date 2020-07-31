using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace BGEditor
{
    public class YSnapController : CoreMonoBehaviour
    {
        public int[] Ys;
        public int DefaultIndex;
        public Slider YPosSlider;
        public Text YPosText;
        public Toggle YFilter;

        private Dropdown dropdown;

        private void HandleSnapChange(int index)
        {
            int target = index == 0 ? 0 : Ys[index - 1];
            if (Editor.yDivision == target)
                return;
            Core.Commit(new ChangeYSnapCmd(target));
        }

        private void HandleYChange(float newY)
        {
            float target = Editor.yDivision == 0 ? float.NaN : newY / Editor.yDivision;
            if (Editor.yDivision > 0 && Mathf.Approximately(Editor.yPos, target))
                return;
            Core.Commit(new ChangeYLayerCmd(target));
        }

        private void RefreshText()
        {
            // Refresh slider
            if (!Mathf.Approximately(Editor.yPos, YPosSlider.value / Editor.yDivision))
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
            dropdown.value = DefaultIndex;

            // Slider
            YPosSlider.onValueChanged.AddListener(HandleYChange);
            YFilter.onValueChanged.AddListener(HandleToggle);
            Core.onYSnapModified.AddListener(RefreshSnap);
            Core.onYPosModified.AddListener(RefreshText);
        }
    }
}
