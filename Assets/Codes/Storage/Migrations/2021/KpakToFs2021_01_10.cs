using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;
using System.Linq;
using Zenject;
using System.IO.Compression;
using System.IO;
using System;

namespace BanGround.Database.Migrations
{
    public class KpakToFs2021_01_10 : MigrationBase
    {
        [Inject]
        private IFileSystem fs;
        [Inject]
        private IDatabaseAPI db;
        [Inject]
        private IDataLoader dataLoader;

        public override int Id => 1;
        public override string Description => "Migrate kpak to local file system";

#pragma warning disable CS0618
        public override async UniTask<bool> Commit()
        {
            Progress = 0;

            #region kpak
            var legacyFs = new KiraFilesystem();
            legacyFs.Init();
            legacyFs.AddSearchPath(DataLoader.FSDir);
            await UniTask.WaitForEndOfFrame();

            var kpaks = legacyFs.GetPackPaths().Where(path => path.EndsWith(".kpak"));
            var kpakStep = 0.8f / kpaks.Count();

            await UniTask.WaitForEndOfFrame();
            float tempProgress = 0;

            foreach (var kpak in kpaks)
            {
                legacyFs.RemoveSearchPath(kpak);
                var task = new ExtractKirapackTask(kpak, fs, db, dataLoader);
                var unitask = task.Run();
                while (unitask.Status == UniTaskStatus.Pending)
                {
                    Progress = tempProgress + task.Progress * kpakStep;
                    await UniTask.WaitForEndOfFrame();
                }
                tempProgress += kpakStep;
                Progress = tempProgress;
            }
            #endregion

            #region database
            var dbStep = 1 - Progress;
            var prevProgress = Progress;
            var dbTask = new HardRefreshSongListTask(db, dataLoader, fs);
            var uniTask = dbTask.Run();

            while (uniTask.Status == UniTaskStatus.Pending)
            {
                Progress = prevProgress + dbStep * dbTask.Progress;
                await UniTask.WaitForEndOfFrame();
            }
            #endregion

            Progress = 1;
            return true;
        }
#pragma warning restore CS0618
    }
}
