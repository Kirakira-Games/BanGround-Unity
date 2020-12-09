using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BGEditor
{
    public class ToolController : MonoBehaviour
    {
        [Inject]
        IChartCore Core;
        [Inject]
        IEditorInfo Editor;

        public Toggle[] Toggles;

        protected void Start()
        {
            Toggles[(int)Editor.tool].isOn = true;
            for (int i = 0; i < Toggles.Length; i++)
            {
                EditorTool tool = (EditorTool)i;
                var toggle = Toggles[i];
                toggle.onValueChanged.AddListener((ison) =>
                {
                    if (ison)
                        Core.SwitchTool(tool);
                });
            }
        }

        public void SwitchTool(int tool)
        {
            Toggles[tool].isOn = true;
        }
    }
}
