using BanGround.Community;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Assets.Codes.Community
{
    public class StoreController : MonoBehaviour
    {
        [Inject(Id = "BanGround")]
        private IStoreProvider store;

        public Button BackBtn;
        public Button SearchBtn;
        public InputField SearchBar;
        public GameObject SongPrefab;
        public ScrollRect StoreRect;

        public int Offset { get; private set; }
        public const int LIMIT = 9;

        private GridLayoutGroup StoreView;
        private List<Song> mSongs = new List<Song>();
        private GameObject mLoadingDisplay;
        private bool isSearching;
        private bool hasMorePages;

        private async UniTaskVoid Search(int offset = 0)
        {
            isSearching = true;
            store.Cancel();
            Offset = offset;
            if (offset == 0)
            {
                mSongs.ForEach(song => Destroy(song.gameObject));
                hasMorePages = true;
                mSongs.Clear();
            }
            try
            {
                // Display loading
                mLoadingDisplay.transform.SetAsLastSibling();
                mLoadingDisplay.SetActive(true);
                var songs = await store.Search(SearchBar.text, offset, LIMIT);
                // Handle infinite scroll
                if (songs.Count < LIMIT)
                    hasMorePages = false;
                Offset += songs.Count;
                mLoadingDisplay.SetActive(false);
                // Create song items
                foreach (var song in songs)
                {
                    var obj = Instantiate(SongPrefab, StoreView.transform).GetComponent<Song>();
                    obj.SetSongItem(song);
                    mSongs.Add(obj);
                }
            }
            catch (OperationCanceledException) { }
            isSearching = false;
        }

        void Start()
        {
            // Get loading display
            StoreView = StoreRect.viewport.GetComponentInChildren<GridLayoutGroup>();
            mLoadingDisplay = StoreView.transform.GetChild(0).gameObject;
            mLoadingDisplay.SetActive(false);
            
            // Initial search
            SearchBtn.onClick.AddListener(() => Search().Forget());
            Search().Forget();
        }

        private void Update()
        {
            if (hasMorePages && !isSearching && StoreRect.normalizedPosition.y < 0)
            {
                // Infinite scroll
                Search(Offset).Forget();
            }
        }
    }
}