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

        public RankItem[] GetRankItems(int sid, Difficulty difficulty)
        {
            var items = DB.RankItemTable.FindByChartIdAndDifficulty((sid, difficulty));
            return items.ToArray();
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
            var builder = DB.ToImmutableBuilder();
            builder.Diff(new RankItem[] { item });
            DB = builder.Build();
            Save();
        }

        public void RemoveRankItem(RankItem item)
        {
            var builder = DB.ToImmutableBuilder();
            builder.RemoveRankItem(new int[] { item.Id });
            DB = builder.Build();
            Save();
        }
    }
}