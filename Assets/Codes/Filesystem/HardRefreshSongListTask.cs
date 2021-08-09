using BanGround.Database;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BanGround
{
    public class HardRefreshSongListTask : ITaskWithProgress
    {
        public float Progress { get; private set; }

        private readonly IDatabaseAPI db;
        private readonly IDataLoader dataLoader;
        private readonly IFileSystem fs;

        public HardRefreshSongListTask(IDatabaseAPI db, IDataLoader dataLoader, IFileSystem fs)
        {
            this.db = db;
            this.dataLoader = dataLoader;
            this.fs = fs;
        }

        public async UniTask Run()
        {
            Progress = 0;
            var files = fs.Find(file => file.Name.EndsWith("cheader.bin")).ToArray();
            await UniTask.WaitForEndOfFrame();
            Progress = 0.05f;
            float step = 0.95f / files.Length;
            
            foreach (var file in files)
            {
                try
                {
                    var header = ProtobufHelper.Load<V2.cHeader>(file);
                    header.LoadDifficultyLevels(dataLoader);
                    db.SaveChartSet(header.sid, header.mid, header.difficultyLevel.ToArray());
                    Progress += step;
                    await UniTask.WaitForEndOfFrame();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Progress = 1;
        }
    }
}
