using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BanGround.Database.Migrations
{
    public class MigrationManager
    {
        [Inject]
        private DiContainer diContainer;

        private const string MIGRATION_ID_KEY = "MigrationId";
        private static readonly Type[] MIGRATIONS = new Type[]
        {
            typeof(KpakToFs2021_01_10)
        };

        private List<MigrationBase> validMigrations;
        private int MigrationIdKey
        {
            get => PlayerPrefs.GetInt(MIGRATION_ID_KEY);
            set
            {
                PlayerPrefs.SetInt(MIGRATION_ID_KEY, value);
                PlayerPrefs.Save();
            }
        }

        public int TotalMigrations => validMigrations?.Count ?? 0;
        public int CurrentMigrationIndex { get; private set; } = 0;
        public float CurrentMigrationProgress
        {
            get
            {
                if (CurrentMigrationIndex >= TotalMigrations)
                    return 1f;

                return validMigrations[CurrentMigrationIndex].Progress;
            }
        }

        /// <returns>Whether any migration is available.</returns>
        public bool Init()
        {
            int currentId = MigrationIdKey;
            validMigrations = MIGRATIONS.Select(migration => diContainer.Instantiate(migration) as MigrationBase)
                .Where(migration => migration.Id > currentId)
                .ToList();
            return validMigrations.Count > 0;
        }

        /// <returns>Whether the operation is successful.</returns>
        public async UniTask<bool> Migrate()
        {
            if (validMigrations.Count <= 0)
                return false;

            validMigrations.Sort((lhs, rhs) => lhs.Id - rhs.Id);
            try
            {
                CurrentMigrationIndex = 0;
                foreach (var migration in validMigrations)
                {
                    if (!await migration.Commit())
                    {
                        return false;
                    }
                    MigrationIdKey = migration.Id;
                    CurrentMigrationIndex++;
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
                return false;
            }
        }
    }
}