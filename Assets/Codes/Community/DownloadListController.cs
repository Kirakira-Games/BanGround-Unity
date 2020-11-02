using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BanGround.Community
{
    [RequireComponent(typeof(Button))]
    public class DownloadListController : MonoBehaviour
    {
        public GameObject DownloadList;
        public Text NumberOfTaskText;
        public Button BackButton;
        public VerticalLayoutGroup Content;
        public GameObject DownloadListItemPrefab;
        public ProgressBar OverallDownloadProgressBar;

        private List<DownloadListItem> mChildren = new List<DownloadListItem>();

        [Inject]
        private IDownloadManager downloadManager;

        private void AddTask(IDownloadTask task)
        {
            var item = Instantiate(DownloadListItemPrefab, Content.transform).GetComponent<DownloadListItem>();
            item.SetDownloadTask(task);
            mChildren.Add(item);
        }

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                DownloadList.SetActive(true);
            });
            BackButton.onClick.AddListener(() =>
            {
                DownloadList.SetActive(false);
            });
            downloadManager.onAddTask.AddListener(AddTask);
        }

        private void Update()
        {
            mChildren = mChildren.Where(child => child != null).ToList();
            NumberOfTaskText.text = mChildren.Where(child =>
                child.DownloadTask.State == DownloadState.Downloading ||
                child.DownloadTask.State == DownloadState.Preparing
            ).Count().ToString();
            OverallDownloadProgressBar.Progress = 0;
            foreach (var child in mChildren)
            {
                if (child.DownloadTask.State == DownloadState.Downloading)
                {
                    OverallDownloadProgressBar.Progress = child.DownloadTask.Progress;
                    break;
                }
            }
        }
    }
}