using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BanGround.Community
{
    public class DownloadListItem : MonoBehaviour
    {
        private static readonly Color SUCCESS_COLOR = new Color(0, 1, 0, .5f);
        private static readonly Color FAILURE_COLOR = new Color(1, 0, 0, .5f);
        public IDownloadTask DownloadTask;
        public Text SongName;
        public Text Progress;
        public Image DownloadImage;
        public Button RemoveButton;
        public ProgressBar Bar;
        private bool hasCreatedImage = false;

        private void OnFinsish()
        {
            Progress.text = "Done";
            Bar.Progress = 1;
            Bar.SetColor(SUCCESS_COLOR);
            RemoveButton.interactable = true;
            RemoveButton.onClick.AddListener(OnRemove);
        }

        private void OnCancel()
        {
            Progress.text = "Failed";
            Bar.Progress = 1;
            Bar.SetColor(FAILURE_COLOR);
            RemoveButton.interactable = true;
            RemoveButton.onClick.AddListener(OnRemove);
        }

        public void SetDownloadTask(IDownloadTask task)
        {
            RemoveButton.interactable = false;
            DownloadTask = task;
            task.OnFinish.AddListener(OnFinsish);
            task.OnCancel.AddListener(OnCancel);
        }

        private void OnDestroy()
        {
            DownloadTask.OnFinish.RemoveListener(OnFinsish);
            DownloadTask.OnCancel.RemoveListener(OnCancel);
        }

        private void OnRemove()
        {
            Destroy(gameObject);
        }

        private void Update()
        {
            if (DownloadTask == null || DownloadTask.State != DownloadState.Downloading)
                return;
            SongName.text = DownloadTask.Name;
            if (DownloadTask.Image != null && !hasCreatedImage)
            {
                var tex = DownloadTask.Image;
                DownloadImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
                hasCreatedImage = true;
            }
            Progress.text = DownloadTask.Progress.ToString("P2");
            Bar.Progress = DownloadTask.Progress;
        }
    }
}