﻿using Cysharp.Threading.Tasks;
using System;

namespace BanGround.Database.Migrations
{
    public abstract class MigrationBase
    {
        public abstract int Id { get; }
        public abstract string Description { get; }
        public virtual float Progress { get; protected set; }

        /// <returns>Whether the operation is successful.</returns>
        public abstract UniTask<bool> Commit();

        /// <returns>Whether the operation is successful.</returns>
        public UniTask<bool> Revert()
        {
            throw new NotImplementedException("This migration cannot be reverted.");
        }
    }
}