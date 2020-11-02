using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BanGround.Community
{
    public class DownloadListController : MonoBehaviour
    {
        public Text NumberOfTaskText;
        public Button DownloadViewButton;
        public Button BackButton;
        public VerticalLayoutGroup Content;
        public GameObject DownloadListItemPrefab;
        public ProgressBar OverallDownloadProgressBar;

        private List<DownloadListItem> mChildren = new List<DownloadListItem>();

        [Inject]
        private IDownloadManager downloadManager;

        private void AddTask(IDownloadTask task)
        {
            var item = Instantiate(DownloadListItemPrefab, Content.transform).GetComponentInChildren<DownloadListItem>();
            item.SetDownloadTask(task);
            mChildren.Add(item);
        }

        private void Start()
        {
            DownloadViewButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(true);
            });
            BackButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
            downloadManager.onAddTask.AddListener(AddTask);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            mChildren = mChildren.Where(child => child.gameObject != null).ToList();
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