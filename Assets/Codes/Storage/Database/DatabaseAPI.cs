using BanGround.Database.Models;
using BanGround.Database.Generated;
using System.IO;
using UnityEngine;
using System.Linq;
using MessagePack;

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