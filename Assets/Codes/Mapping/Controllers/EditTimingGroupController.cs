using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx.Async;
using Zenject;

namespace BGEditor
{
    public class EditTimingGroupController : MonoBehaviour
    {
        [Inject]
        private IChartCore Core;
        [Inject]
        private IEditorInfo Editor;
        [Inject]
        private IMessageBannerController messageBannerController;
        [Inject]
        private IMessageBox messageBox;

        public Dropdown groupDropdown;
        public Button deleteButton;

        public const int MAX_TIMING_GROUP = 9;

        private async UniTaskVoid OnDelete()
        {
            if (Core.chart.groups.Count <= 1)
            {
                messageBannerController.ShowMsg(LogLevel.INFO, "You cannot remove the last timing group.");
                return;
            }
            if (!await messageBox.ShowMessage("Remove Timing Group", "Confirm: Current timing group, notes, and speed information will be lost."))
            {
                return;
            }
            Core.Commit(new RemoveTimingGroupCmd(Editor, Editor.currentTimingGroup));
        }

        public void Add()
        {
            if (Core.chart.groups.Count >= MAX_TIMING_GROUP)
            {
                Debug.LogWarning("User is not allowed to have more than 9 timing groups.");
                return;
            }
            Core.Commit(new AddTimingGroupCmd(Editor));
        }

        private void Refresh()
        {
            groupDropdown.SetValueWithoutNotify(0);
            groupDropdown.ClearOptions();
            for (int i = 1; i <= Core.chart.groups.Count; i++)
            {
                groupDropdown.options.Add(new Dropdown.OptionData(i.ToString()));
            }
            if (Core.chart.groups.Count < MAX_TIMING_GROUP)
            {
                groupDropdown.options.Add(new Dropdown.OptionData("New..."));
            }
            groupDropdown.SetValueWithoutNotify(Editor.currentTimingGroup);
            groupDropdown.captionText.text = (Editor.currentTimingGroup + 1).ToString();
            Core.onTimingGroupSwitched.Invoke();
        }

        public void Switch(int index)
        {
            if (index < Core.chart.groups.Count)
            {
                if (groupDropdown.value != index)
                {
                    groupDropdown.SetValueWithoutNotify(index);
                }
                Editor.currentTimingGroup = index;
                Core.onTimingGroupSwitched.Invoke();
                return;
            }
            if (index == Core.chart.groups.Count)
            {
                Add();
                return;
            }
        }

        private void Awake()
        {
            deleteButton.onClick.AddListener(() => _ = OnDelete());
            groupDropdown.onValueChanged.AddListener(Switch);
            Core.onTimingGroupModified.AddListener(Refresh);
            Core.onChartLoaded.AddListener(Refresh);
        }
    }
}