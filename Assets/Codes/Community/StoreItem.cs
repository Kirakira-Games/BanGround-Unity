using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using Zenject;
using BanGround.Web;

namespace BanGround.Community
{
    public class StoreItem : MonoBehaviour
    {
        [Inject(Id = "cl_lastsid")]
        private KVar cl_lastsid;
        [Inject]
        private IStoreController controller;
        [Inject]
        private IMessageBannerController messageBanner;
        [Inject]
        private IMessageBox messageBox;
        [Inject]
        private IMessageCenter messageCenter;
        [Inject]
        private IResourceDownloadCache<Texture2D> textureCache;
        [Inject]
        private ILoadingBlocker loadingBlocker;

        public Text Title;
        public Text Artist;
        public GameObject Difficulty;
        public GameObject DiffRange;
        public Image Background;
        public Sprite DefaultImage;

        public SongItem SongItem { get; private set; }
        public ChartItem ChartItem { get; private set; }

        private GameObject[] diffs;
        private Text[] diffTexts;
        private Text diffRangeText;

        public void OnClick()
        {
            if (SongItem == null)
            {
                Download().Forget();
            }
            else
            {
                controller.LoadCharts(SongItem, 0).Forget();
            }
        }

        private async UniTaskVoid Download()
        {
            if (ChartItem == null)
            {
                Debug.LogError("Unable to download chart: Chart item is not specified");
                return;
            }
            var song = controller.ViewStack.Peek().Song;
            if (song == null)
            {
                Debug.LogError("Unable to download chart: Song item is not specified");
                return;
            }
            // User confirm
            if (!await messageBox.ShowMessage("Download Chart", $"{song.ToDisplayString()}\n{ChartItem.ToDisplayString()}"))
            {
                return;
            }
            // Start download
            loadingBlocker.Show("Creating download task...");
            try
            {
                var task = await controller.StoreProvider.AddToDownloadList(song, ChartItem);
                var id = IDRouterUtil.ToFileId(ChartItem.Source, ChartItem.Id);
                task.SetImage(Background.sprite.texture)
                    .SetName($"{song.Title} ({ChartItem.Uploader.Nickname})")
                    .OnFinish.AddListener(() =>
                    {
                        cl_lastsid.Set(id);
                    });
                messageCenter.Show("Download", "Go");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
                messageBanner.ShowMsg(LogLevel.ERROR, e.Message);
            }
            loadingBlocker.Close();
        }

        private void Awake()
        {
            Background.GetComponent<Button>().onClick.AddListener(OnClick);
            diffs = new GameObject[Difficulty.transform.childCount];
            diffTexts = new Text[diffs.Length];
            for (int i = 0; i < diffs.Length; i++)
            {
                diffs[i] = Difficulty.transform.GetChild(i).gameObject;
                diffTexts[i] = diffs[i].GetComponentInChildren<Text>();
            }
            diffRangeText = DiffRange.GetComponentInChildren<Text>();
        }

        private async UniTaskVoid GetImage()
        {
            Background.sprite = DefaultImage;
            string url = SongItem?.BackgroundUrl ?? ChartItem?.BackgroundUrl;
            if (string.IsNullOrEmpty(url))
                return;
            try
            {
                var tex = await textureCache.Fetch(url);
                if (this != null && tex != null)
                    Background.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
            }
            catch (OperationCanceledException) { }
            catch (UnityWebRequestException) { }
        }

        public void SetItem(SongItem item)
        {
            ChartItem = null;
            SongItem = item;
            GetImage().Forget();

            // Title and artist
            Title.text = item.Title;
            Artist.gameObject.SetActive(true);
            Artist.text = item.Artist;

            // Difficulty
            Difficulty.SetActive(false);
            DiffRange.SetActive(true);
            if (item.DiffRange.Length == 2)
            {
                diffRangeText.text = $"{item.DiffRange[0]} - {item.DiffRange[1]}";
            }
            else
            {
                diffRangeText.text = "Empty";
            }
        }

        public void SetItem(ChartItem item)
        {
            SongItem = null;
            ChartItem = item;
            GetImage().Forget();

            // Title and artist
            Artist.gameObject.SetActive(false);
            Title.text = "By " + item.Uploader.Nickname;

            // Difficulty
            Difficulty.SetActive(true);
            DiffRange.SetActive(false);
            for (int i = 0; i < item.Difficulty.Count; i++)
            {
                if (item.Difficulty[i] == -1)
                {
                    diffs[i].SetActive(false);
                }
                else
                {
                    diffTexts[i].text = item.Difficulty[i].ToString();
                    diffs[i].SetActive(true);
                }
            }
        }
    }
}