using BanGround.Database.Models;
using BanGround.Database.Generated;
using System.IO;
using UnityEngine;
using System.Linq;
using MessagePack;
using System;

namespace BanGround.Database
{
    public class DatabaseAPI : IDatabaseAPI
    {
        public readonly string DBConnection = Application.persistentDataPath + "/Scores.bin";
        public MemoryDatabase DB { get; private set; }

        public DatabaseAPI()
        {
            Reload();
        }

        private void Save(DatabaseBuilder builder)
        {
            using (var writeStream = File.OpenWrite(DBConnection))
            {
                builder.WriteToStream(writeStream);
            }
        }

        public void Save()
        {
            Save(DB.ToDatabaseBuilder());
        }

        public void Reload()
        {
            if (File.Exists(DBConnection))
            {
                try
                {
                    DB = new MemoryDatabase(File.ReadAllBytes(DBConnection));
                }
                catch (MessagePackSerializationException)
                {
                    File.Delete(DBConnection);
                    Reload();
                }
            }
            else
            {
                DB = new MemoryDatabase(new DatabaseBuilder().Build());
            }
        }

        private delegate void DatabaseModificationOperation(ImmutableBuilder builder);
        private void ModifyDB(DatabaseModificationOperation operation, bool autoSave = true)
        {
            var builder = DB.ToImmutableBuilder();
            operation(builder);
            DB = builder.Build();
            if (autoSave)
                Save();
        }

        #region RankItem
        public RankItem[] GetRankItems(int sid, Difficulty difficulty)
        {
            var items = DB.RankItemTable.FindByChartIdAnd_Difficulty((sid, (int)difficulty)).ToArray();
            foreach (var item in items)
            {
                item.ClearMark = RankItem.GetClearMark(item.Judge, item.Combo, item.Acc);
            }
            return items;
        }

        public RankItem GetBestRank(int sid, Difficulty difficulty)
        {
            var items = GetRankItems(sid, difficulty);
            if (items.Length == 0)
                return null;
            var best = new RankItem();
            foreach (var item in items)
            {
                if (item.Score >= best.Score)
                {
                    best.Score = item.Score;
                    best.Judge = item.Judge;
                    best.MusicId = item.MusicId;
                    best.CreatedAt = item.CreatedAt;

                    best.Mods = item.Mods;
                    best.ChartHash = item.ChartHash;
                    best.ReplayFile = item.ReplayFile;
                }
                if ((int)item.ClearMark < (int)best.ClearMark)
                {
                    best.ClearMark = item.ClearMark;
                }
                best.Acc = Math.Max(item.Acc, best.Acc);
                best.Combo = Math.Max(item.Combo, best.Combo);
            }
            return best;
        }

        public void SaveRankItem(RankItem item)
        {
            if (item.Id == 0)
            {
                if (DB.RankItemTable.Count == 0)
                {
                    item.Id = 1;
                }
                else
                {
                    item.Id = DB.RankItemTable.All.Last.Id + 1;
                }
            }
            ModifyDB((builder) => builder.Diff(new RankItem[] { item }));
        }

        public void RemoveRankItem(RankItem item)
        {
            ModifyDB((builder) => builder.RemoveRankItem(new int[] { item.Id }));
        }
        #endregion

        #region ChartSet
        public ChartSet[] GetAllChartSets()
        {
            return DB.ChartSetTable.All.ToArray();
        }

        public ChartSet GetChartSetBySid(int sid)
        {
            return DB.ChartSetTable.FindBySid(sid);
        }

        public ChartSet[] GetChartSetsByMid(int mid)
        {
            return DB.ChartSetTable.FindByMid(mid).ToArray();
        }

        public void RegisterChartSet(int sid, int mid, int[] difficulties)
        {
            if (difficulties == null ||
                difficulties.Length != (int)Difficulty.Special + 1 ||
                difficulties.All(diff => diff == -1))
            {
                Debug.LogError("[RegisterChartSet] Difficulties are not provided or malformed.");
                return;
            }
            var item = new ChartSet
            {
                Sid = sid,
                Mid = mid,
                Difficulties = difficulties
            };
            ModifyDB((builder) => builder.Diff(new ChartSet[] { item }));
        }

        public void RemoveChartSets(int[] sids)
        {
            ModifyDB((builder) => builder.RemoveChartSet(sids));
        }

        public void RemoveChartSetDifficulty(int sid, Difficulty difficulty)
        {
            int diff = (int)difficulty;
            var chart = GetChartSetBySid(sid);
            if (chart == null)
            {
                Debug.LogError($"[Remove Chart Set] Chart set {sid} does not exist.");
                return;
            }
            if (chart.Difficulties[diff] == -1)
            {
                Debug.LogError($"[Remove Chart Set] Chart set {sid} does not have difficulty {difficulty}.");
                return;
            }
            chart.Difficulties[diff] = -1;
            if (chart.Difficulties.All(d => d == -1))
            {
                // All difficulties are already removed
                RemoveChartSets(new int[] { sid });
            }
            else
            {
                ModifyDB((builder) => builder.Diff(new ChartSet[] { chart }));
            }
        }
        #endregion
    }
}