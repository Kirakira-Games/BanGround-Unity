using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BGEditor
{
    public class ToolController : CoreMonoBehaviour
    {
        public Toggle[] Toggles;

        protected override void Awake()
        {
            base.Awake();
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
    }
}
