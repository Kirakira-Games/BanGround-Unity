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
            float target = Editor.yDivision == 0 ? 0 : newY / Editor.yDivision;
            if (Mathf.Approximately(Editor.yPos, target))
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

        private void RefreshSnap()
        {
            // Refresh dropdown
            if (Editor.yDivision == 0)
            {
                dropdown.SetValueWithoutNotify(0);
            }
            else
            {
                for (int i = 0; i < Ys.Length; i++)
                {
                    if (Ys[i] == Editor.yDivision)
                    {
                        dropdown.SetValueWithoutNotify(i + 1);
                        break;
                    }
                }
            }

            // Find nearest
            YPosSlider.maxValue = Editor.yDivision;
            if (Editor.yDivision == 0)
            {
                if (YPosSlider.value > 0)
                {
                    YPosSlider.value = 0;
                }
                RefreshText();
                return;
            }
            int mini = 0;
            for (int i = 1; i <= Editor.yDivision; i++)
            {
                if (Mathf.Abs((float)i / Editor.yDivision - Editor.yPos)
                    < Mathf.Abs((float)mini / Editor.yDivision - Editor.yPos))
                {
                    mini = i;
                }
            }
            YPosSlider.value = mini;
            RefreshText();
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
            Core.onYSnapModified.AddListener(RefreshSnap);
            Core.onYPosModified.AddListener(RefreshText);
        }
    }
}
