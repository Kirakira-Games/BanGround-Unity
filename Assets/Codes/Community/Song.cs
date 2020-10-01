using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;
using System;

namespace BanGround.Community
{
    public class Song : MonoBehaviour
    {
        public Text Title;
        public Image Background;
        public Sprite DefaultImage;

        public SongItem SongItem { get; private set; }
        private CancellationTokenSource mTokenSource = new CancellationTokenSource();

        private void OnClick()
        {
            Debug.Log(SongItem);
        }

        private void Start()
        {
            Background.GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private async UniTaskVoid GetImage()
        {
            Background.sprite = DefaultImage;
            if (string.IsNullOrEmpty(SongItem.BackgroundUrl))
                return;
            try
            {
                using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(SongItem.BackgroundUrl))
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

        public void SetSongItem(SongItem item)
        {
            SongItem = item;
            GetImage().Forget();
            Title.text = item.Title;
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