using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Zenject;
using System.Linq;

namespace BGEditor
{
    public class BeatSnapController : MonoBehaviour
    {
        [Inject]
        IChartCore Core;
        [Inject]
        IEditorInfo Editor;

        public int[] Beats;

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
            Core.onChartLoaded.AddListener(() =>
            {
                dropdown.value = Beats.ToList().IndexOf(Editor.gridDivision);
            });
        }
    }
}
