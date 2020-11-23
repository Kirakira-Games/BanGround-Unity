using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using Zenject;

namespace BanGround.Community
{
    public enum StoreViewType
    {
        Song,
        Chart
    }

    public class StoreViewState
    {
        public StoreViewType Type;
        public List<StoreItem> Items = new List<StoreItem>();
        public float NormalizedY = 0;
        public int Offset = 0;
        public bool HasMorePages = true;
        public string SearchText = "";
        public string Title = "";
        public SongItem Song;
    }

    public class StoreStack
    {
        private StoreController mController;
        private readonly Stack<StoreViewState> mStack = new Stack<StoreViewState>();

        public StoreStack(StoreController controller)
        {
            mController = controller;
        }

        public int Count => mStack.Count;
        public StoreViewState Peek() => Count > 0 ? mStack.Peek() : null;

        public void RefreshState()
        {
            if (Count == 0)
                return;
            var state = Peek();
            mController.StoreRect.normalizedPosition = new Vector2(mController.StoreRect.normalizedPosition.x, state.NormalizedY);
            mController.SearchBar.text = state.SearchText;
            mController.TitleText.text = state.Title;
        }

        public StoreViewState Pop()
        {
            var ret = mStack.Pop();
            ret.Items.ForEach(item => GameObject.Destroy(item.gameObject));
            if (mStack.Count > 0)
            {
                Peek().Items.ForEach(item => item.gameObject.SetActive(true));
            }
            return ret;
        }

        public StoreViewState Create(StoreViewType type)
        {
            if (mStack.Count > 0)
            {
                Peek().Items.ForEach(item => item.gameObject.SetActive(false));
            }
            var ret = new StoreViewState();
            ret.Type = type;
            mStack.Push(ret);
            return ret;
        }

        public void Clear()
        {
            while (mStack.Count > 0)
                Pop();
        }
    }

    public class StoreController : MonoBehaviour, IStoreController
    {
        [Inject(Id = "BanGround")]
        private IStoreProvider bgStore;
        [Inject]
        private DiContainer container;

        public Button BackBtn;
        public Button SearchBtn;
        public InputField SearchBar;
        public GameObject SongPrefab;
        public ScrollRect StoreRect;
        public Text TitleText;

        public const int LIMIT = 9;

        private GameObject mLoadingDisplay;

        public StoreStack ViewStack { get; private set; }
        public GridLayoutGroup StoreView { get; private set; }
        public bool isLoading { get; private set; }

        public IStoreProvider StoreProvider => bgStore;

        private void OnBackButtonClicked()
        {
            ViewStack.Pop();
            if (ViewStack.Count == 0)
            {
                SceneLoader.LoadScene("Community", "Title");
            }
            else
            {
                ViewStack.RefreshState();
            }
        }

        public async UniTaskVoid LoadCharts(SongItem song, int offset)
        {
            isLoading = true;
            StoreProvider.Cancel();
            var state = ViewStack.Create(StoreViewType.Chart);
            state.SearchText = SearchBar.text;
            state.Title = song.Title;
            state.Song = song;

            // Fetch from API
            mLoadingDisplay.transform.SetAsLastSibling();
            mLoadingDisplay.SetActive(true);

            try
            {
                var charts = await StoreProvider.GetCharts(song.Id, offset, LIMIT);

                // Handle infinite scroll
                if (charts.Count < LIMIT)
                    state.HasMorePages = false;
                state.Offset += charts.Count;

                // Create song items
                foreach (var chart in charts)
                {
                    var obj = container.InstantiatePrefab(SongPrefab, StoreView.transform).GetComponent<StoreItem>();
                    obj.SetItem(chart);
                    state.Items.Add(obj);
                }
            }
            catch (OperationCanceledException) { }

            // Finish
            mLoadingDisplay.SetActive(false);
            ViewStack.RefreshState();
            isLoading = false;
        }

        public async UniTaskVoid Search(string text, int offset = 0)
        {
            isLoading = true;
            StoreProvider.Cancel();
            if (offset == 0)
            {
                ViewStack.Clear();
                var state = ViewStack.Create(StoreViewType.Song);
                state.SearchText = text;
                state.Title = text.IsNullOrEmpty() ? "Community" : $"Search: {text}";
                ViewStack.RefreshState();
            }
            try
            {
                var state = ViewStack.Peek();
                state.Offset = offset;
                // Display loading
                mLoadingDisplay.transform.SetAsLastSibling();
                mLoadingDisplay.SetActive(true);
                var songs = await StoreProvider.Search(text, offset, LIMIT);
                // Handle infinite scroll
                if (songs.Count < LIMIT)
                    state.HasMorePages = false;
                state.Offset += songs.Count;
                // Create song items
                foreach (var song in songs)
                {
                    var obj = container.InstantiatePrefab(SongPrefab, StoreView.transform).GetComponent<StoreItem>();
                    obj.SetItem(song);
                    state.Items.Add(obj);
                }
            }
            catch (OperationCanceledException) { }
            mLoadingDisplay.SetActive(false);
            isLoading = false;
        }

        void Start()
        {
            ViewStack = new StoreStack(this);

            // Get loading display
            StoreView = StoreRect.viewport.GetComponentInChildren<GridLayoutGroup>();
            mLoadingDisplay = StoreView.transform.GetChild(0).gameObject;
            mLoadingDisplay.SetActive(false);

            // Initial search
            SearchBtn.onClick.AddListener(() => Search(SearchBar.text).Forget());
            Search(SearchBar.text).Forget();

            // Back button
            BackBtn.onClick.AddListener(OnBackButtonClicked);
        }

        private void Update()
        {
            var state = ViewStack.Peek();
            if (state == null)
            {
                return;
            }
            state.NormalizedY = StoreRect.normalizedPosition.y;
            if (state.HasMorePages && !isLoading && state.NormalizedY <= 0.01f)
            {
                // Infinite scroll
                if (state.Type == StoreViewType.Song)
                    Search(SearchBar.text, state.Offset).Forget();
                else
                    LoadCharts(state.Song, state.Offset).Forget();
            }
        }

        private void OnDestroy()
        {
            StoreProvider.Cancel();
        }
    }
}