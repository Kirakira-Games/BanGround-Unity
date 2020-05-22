using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace BGEditor
{
    public class BeatSnapController : CoreMonoBehaviour
    {
        public int[] Beats;
        public int DefaultIndex;

        private Dropdown dropdown;

        private void HandleValueChange(int index)
        {
            Core.SetGridDivision(Beats[index]);
        }

        void Start()
        {
            dropdown = GetComponent<Dropdown>();
            dropdown.ClearOptions();
            foreach (int i in Beats)
            {
                dropdown.options.Add(new Dropdown.OptionData("1 / " + i));
            }
            dropdown.onValueChanged.AddListener(HandleValueChange);
            dropdown.value = DefaultIndex;
        }
    }
}
