using BanGround.Community;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Zenject;

namespace Assets.Codes.Community.Bestdori
{
    class BestdoriStoreProvider : IStoreProvider
    {
        [Inject]
        private BestdoriWebRequest web;

        public UniTask<IDownloadTask> AddToDownloadList(SongItem song, ChartItem chart)
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public async UniTask<List<ChartItem>> GetCharts(int mid, int offset, int limit)
        {
            var chart = (await web.GetChartById(mid).Fetch()).post;
            return new List<ChartItem> { chart.ToChartItem() };
        }

        public UniTask<List<SongItem>> Search(string keyword, int offset, int limit)
        {
            throw new NotImplementedException();
        }
    }
}
