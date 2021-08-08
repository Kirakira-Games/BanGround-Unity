﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BanGround.Database.Migrations
{
    public class MigrationManager : IMigrationManager
    {
        [Inject]
        private DiContainer diContainer;

        private const string MIGRATION_ID_KEY = "MigrationId";
        private static readonly Type[] MIGRATIONS = new Type[]
        {
            typeof(KpakToFs2021_01_10),
            typeof(NewInitalKirapack2021_07_15),
            typeof(ConvertReplayFiles2021_08_08),
            // Next migration ID: 7
        };

        private List<MigrationBase> validMigrations;
        private int MigrationId
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
        public float Progress
        {
            get
            {
                if (CurrentMigrationIndex >= TotalMigrations)
                    return 1f;

                return validMigrations[CurrentMigrationIndex].Progress;
            }
        }
        public string Description
        {
            get
            {
                if (CurrentMigrationIndex >= TotalMigrations)
                    return "Wrapping up...";

                return validMigrations[CurrentMigrationIndex].Description;
            }
        }

        /// <returns>Whether any migration is available.</returns>
        public bool Init()
        {
            //MigrationId = 0;
            int currentId = MigrationId;
            validMigrations = MIGRATIONS.Select(migration => diContainer.Instantiate(migration) as MigrationBase)
                .Where(migration => migration.Id > currentId || migration.ShouldRun())
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
                    MigrationId = Mathf.Max(migration.Id, MigrationId);
                    CurrentMigrationIndex++;
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
    }
}