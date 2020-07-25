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
            Core.SetYDivision(index == 0 ? 0 : Ys[index - 1]);
        }

        private void HandleYChange(float newY)
        {
            Core.SetY(Editor.yDivision == 0 ? 0 : newY / Editor.yDivision);
        }

        private void RefreshText()
        {
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
