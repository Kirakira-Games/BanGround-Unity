﻿using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Networking;
using System;
using Zenject;
using System.Collections.Generic;
using BanGround.Web;

namespace BanGround.Community
{
    public class StoreItem : MonoBehaviour
    {
        [Inject]
        private IStoreController controller;
        [Inject]
        private IMessageBox messageBox;
        [Inject]
        private IMessageCenter messageCenter;
        [Inject]
        private IResourceDownloadCache<Texture2D> textureCache;

        public Text Title;
        public Image Background;
        public Sprite DefaultImage;

        public SongItem SongItem { get; private set; }
        public ChartItem ChartItem { get; private set; }

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
            messageCenter.Show("Download", "Go");
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
                var tex = await textureCache.Fetch(url);
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
            Title.text = item.Title;
        }

        public void SetItem(ChartItem item)
        {
            SongItem = null;
            ChartItem = item;
            GetImage().Forget();
            Title.text = "By " + item.Uploader.Nickname;
        }
    }
}