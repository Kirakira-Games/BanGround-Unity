using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UniRx.Async;

namespace BGEditor
{
    public class EditTimingGroupController : CoreMonoBehaviour
    {
        public Dropdown groupDropdown;
        public Button deleteButton;

        public const int MAX_TIMING_GROUP = 9;

        private async UniTaskVoid OnDelete()
        {
            if (Chart.groups.Count <= 1)
            {
                MessageBannerController.ShowMsg(LogLevel.INFO, "You cannot remove the last timing group.");
                return;
            }
            if (!await MessageBox.ShowMessage("Remove Timing Group", "Confirm: Current timing group, notes, and speed information will be lost."))
            {
                return;
            }
            Core.Commit(new RemoveTimingGroupCmd(Editor.currentTimingGroup));
        }

        public void Add()
        {
            if (Chart.groups.Count >= MAX_TIMING_GROUP)
            {
                Debug.LogWarning("User is not allowed to have more than 9 timing groups.");
                return;
            }
            Core.Commit(new AddTimingGroupCmd());
        }

        private void Refresh()
        {
            groupDropdown.SetValueWithoutNotify(0);
            groupDropdown.ClearOptions();
            for (int i = 1; i <= Chart.groups.Count; i++)
            {
                groupDropdown.options.Add(new Dropdown.OptionData(i.ToString()));
            }
            if (Chart.groups.Count < MAX_TIMING_GROUP)
            {
                groupDropdown.options.Add(new Dropdown.OptionData("New..."));
            }
            groupDropdown.SetValueWithoutNotify(Editor.currentTimingGroup);
            groupDropdown.captionText.text = (Editor.currentTimingGroup + 1).ToString();
            Core.onTimingGroupSwitched.Invoke();
        }

        public void Switch(int index)
        {
            if (index < Chart.groups.Count)
            {
                if (groupDropdown.value != index)
                {
                    groupDropdown.SetValueWithoutNotify(index);
                }
                Editor.currentTimingGroup = index;
                Core.onTimingGroupSwitched.Invoke();
                return;
            }
            if (index == Chart.groups.Count)
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