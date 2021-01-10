using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;
using Zenject;

namespace BanGround.Database.Migrations
{
    public class KpakToFs2021_01_10 : MigrationBase
    {
        [Inject]
        private IFileSystem fs;

        public override int Id => 1;
        public override string Description => "Migrate kpak to local file system";

        public override async UniTask<bool> Commit()
        {
            Progress = 0;
            Debug.Assert(fs != null);
            throw new System.NotImplementedException("TODO: GEEKiDoS");
            Progress = 1;
        }
    }
}