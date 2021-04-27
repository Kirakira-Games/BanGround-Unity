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
    public class NewInitalKirapack2021_04_26 : MigrationBase
    {
        [Inject]
        private IDataLoader dataLoader;

        public override int Id => 4;
        public override string Description => "New initial kirapack ";

        public override async UniTask<bool> Commit()
        {
            Progress = 0;

            await dataLoader.CopyFileFromStreamingAssetsToPersistentDataPath("/Initial.kirapack");

            Progress = 1;
            return true;
        }
    }
}