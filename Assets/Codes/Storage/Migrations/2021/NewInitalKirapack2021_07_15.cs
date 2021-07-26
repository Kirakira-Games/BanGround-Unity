using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BanGround.Database.Migrations
{
    public class NewInitalKirapack2021_07_15 : MigrationBase
    {
        [Inject]
        private IDataLoader dataLoader;

        public override int Id => 5;
        public override string Description => "New initial kirapack ";

        public override async UniTask<bool> Commit()
        {
            Progress = 0;

            await dataLoader.CopyFileFromStreamingAssetsToPersistentDataPath("/Initial.kirapack");

            Progress = 1;
            return true;
        }

        public override bool ShouldRun() {
            if (dataLoader.GetChartHeader(OffsetGuide.OFFSET_GUIDE_SID) == null) {
                Debug.LogWarning("Offset guide is lost, trying to reimport.");
                return true;
            }
            return false;
        }
    }
}