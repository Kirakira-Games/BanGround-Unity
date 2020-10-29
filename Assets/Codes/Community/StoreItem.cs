using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;
using System;
using Zenject;

namespace BanGround.Community
{
    public class StoreItem : MonoBehaviour
    {
        public IStoreController Controller;

        public Text Title;
        public Image Background;
        public Sprite DefaultImage;

        public SongItem SongItem { get; private set; }
        public ChartItem ChartItem { get; private set; }
        private CancellationTokenSource mTokenSource = new CancellationTokenSource();

        public void OnClick()
        {
            if (SongItem == null)
            {
                Debug.Log(ChartItem);
            }
            else
            {
                Controller.LoadCharts(SongItem, 0).Forget();
            }
        }

        private void Start()
        {
            Background.GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private async UniTaskVoid GetImage()
        {
            Background.sprite = DefaultImage;
            string url = SongItem?.BackgroundUrl ?? ChartItem?.BackgroundUrl;
            if (string.IsNullOrEmpty(url))
                return;
            try
            {
                using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
                {
                    await webRequest.SendWebRequest().WithCancellation(mTokenSource.Token);
                    if (webRequest.isHttpError || webRequest.isNetworkError)
                        return;
                    var tex = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                    Background.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
                }
            }
            catch (OperationCanceledException) { }
            catch (UnityWebRequestException) { }
        }

        public void SetItem(SongItem item)
        {
            ChartItem = null;
            SongItem = item;
            GetImage().Forget();
            Title.text = item.Title;
        }

        public void SetItem(ChartItem item)
        {
            SongItem = null;
            ChartItem = item;
            GetImage().Forget();
            Title.text = "By " + item.Uploader.Nickname;
        }

        public void Cancel()
        {
            mTokenSource.Cancel();
            mTokenSource = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            Cancel();
        }
    }
}