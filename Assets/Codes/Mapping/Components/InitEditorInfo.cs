using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BGEditor
{
    public class InitEditorInfo : MonoBehaviour
    {
        [Inject]
        IEditorInfo editor;

        public Toggle SpeedViewToggle;
        public Toggle SEToggle;

        void Start()
        {
            SpeedViewToggle.SetIsOnWithoutNotify(editor.isSpeedView);
            SEToggle.SetIsOnWithoutNotify(editor.isSEOn);
            SEToggle.onValueChanged.AddListener((isOn) => editor.isSEOn = isOn);
        }
    }
}
