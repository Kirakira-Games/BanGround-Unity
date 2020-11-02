using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BanGround.Community
{
    [RequireComponent(typeof(ProgressBar))]
    public class DownloadListItem : MonoBehaviour
    {
        private static readonly Color SUCCESS_COLOR = new Color(0, 1, 0, .5f);
        private static readonly Color FAILURE_COLOR = new Color(1, 0, 0, .5f);
        public IDownloadTask DownloadTask;
        public Text SongName;
        public Text Progress;
        public Image DownloadImage;
        public Button RemoveButton;
        private ProgressBar Bar;

        public void SetDownloadTask(IDownloadTask task)
        {
            DownloadTask = task;
            SongName.text = DownloadTask.Name;
            var tex = task.Image;
            DownloadImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
            task.OnCancel.AddListener(() =>
            {
                Progress.text = "Done";
                Bar.Progress = 1;
                Bar.SetColor(SUCCESS_COLOR);
                RemoveButton.onClick.AddListener(OnRemove);
            });
            task.OnFinish.AddListener(() =>
            {
                Progress.text = "Failed";
                Bar.Progress = 0;
                Bar.SetColor(FAILURE_COLOR);
                RemoveButton.onClick.AddListener(OnRemove);
            });
        }

        private void Start()
        {
            Bar = GetComponent<ProgressBar>();
            RemoveButton.interactable = false;
        }

        private void OnRemove()
        {
            Destroy(gameObject);
        }

        private void Update()
        {
            if (DownloadTask == null || DownloadTask.State != DownloadState.Downloading)
                return;
            Progress.text = DownloadTask.Progress.ToString("P2");
            Bar.Progress = DownloadTask.Progress;
        }
    }
}