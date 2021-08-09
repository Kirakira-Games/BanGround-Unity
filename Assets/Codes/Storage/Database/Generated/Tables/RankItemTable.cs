// <auto-generated />
#pragma warning disable CS0105
using BanGround.Database.Models;
using MasterMemory.Validation;
using MasterMemory;
using MessagePack;
using System.Collections.Generic;
using System;

namespace BanGround.Database.Generated.Tables
{
   public sealed partial class RankItemTable : TableBase<RankItem>, ITableUniqueValidate
   {
        public Func<RankItem, int> PrimaryKeySelector => primaryIndexSelector;
        readonly Func<RankItem, int> primaryIndexSelector;

        readonly RankItem[] secondaryIndex0;
        readonly Func<RankItem, (int ChartId, int _Difficulty)> secondaryIndex0Selector;
        readonly RankItem[] secondaryIndex1;
        readonly Func<RankItem, int> secondaryIndex1Selector;
        readonly RankItem[] secondaryIndex2;
        readonly Func<RankItem, string> secondaryIndex2Selector;
        readonly RankItem[] secondaryIndex3;
        readonly Func<RankItem, DateTime> secondaryIndex3Selector;

        public RankItemTable(RankItem[] sortedData)
            : base(sortedData)
        {
            this.primaryIndexSelector = x => x.Id;
            this.secondaryIndex0Selector = x => (x.ChartId, x._Difficulty);
            this.secondaryIndex0 = CloneAndSortBy(this.secondaryIndex0Selector, System.Collections.Generic.Comparer<(int ChartId, int _Difficulty)>.Default);
            this.secondaryIndex1Selector = x => x.MusicId;
            this.secondaryIndex1 = CloneAndSortBy(this.secondaryIndex1Selector, System.Collections.Generic.Comparer<int>.Default);
            this.secondaryIndex2Selector = x => x.ChartHash;
            this.secondaryIndex2 = CloneAndSortBy(this.secondaryIndex2Selector, System.StringComparer.Ordinal);
            this.secondaryIndex3Selector = x => x.CreatedAt;
            this.secondaryIndex3 = CloneAndSortBy(this.secondaryIndex3Selector, System.Collections.Generic.Comparer<DateTime>.Default);
            OnAfterConstruct();
        }

        partial void OnAfterConstruct();

        public RangeView<RankItem> SortByChartIdAnd_Difficulty => new RangeView<RankItem>(secondaryIndex0, 0, secondaryIndex0.Length - 1, true);
        public RangeView<RankItem> SortByMusicId => new RangeView<RankItem>(secondaryIndex1, 0, secondaryIndex1.Length - 1, true);
        public RangeView<RankItem> SortByChartHash => new RangeView<RankItem>(secondaryIndex2, 0, secondaryIndex2.Length - 1, true);
        public RangeView<RankItem> SortByCreatedAt => new RangeView<RankItem>(secondaryIndex3, 0, secondaryIndex3.Length - 1, true);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public RankItem FindById(int key)
        {
            var lo = 0;
            var hi = data.Length - 1;
            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                var selected = data[mid].Id;
                var found = (selected < key) ? -1 : (selected > key) ? 1 : 0;
                if (found == 0) { return data[mid]; }
                if (found < 0) { lo = mid + 1; }
                else { hi = mid - 1; }
            }
            return ThrowKeyNotFound(key);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool TryFindById(int key, out RankItem result)
        {
            var lo = 0;
            var hi = data.Length - 1;
            while (lo <= hi)
            {
                var mid = (int)(((uint)hi + (uint)lo) >> 1);
                var selected = data[mid].Id;
                var found = (selected < key) ? -1 : (selected > key) ? 1 : 0;
                if (found == 0) { result = data[mid]; return true; }
                if (found < 0) { lo = mid + 1; }
                else { hi = mid - 1; }
            }
            result = default;
            return false;
        }

        public RankItem FindClosestById(int key, bool selectLower = true)
        {
            return FindUniqueClosestCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<RankItem> FindRangeById(int min, int max, bool ascendant = true)
        {
            return FindUniqueRangeCore(data, primaryIndexSelector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

        public RangeView<RankItem> FindByChartIdAnd_Difficulty((int ChartId, int _Difficulty) key)
        {
            return FindManyCore(secondaryIndex0, secondaryIndex0Selector, System.Collections.Generic.Comparer<(int ChartId, int _Difficulty)>.Default, key);
        }

        public RangeView<RankItem> FindClosestByChartIdAnd_Difficulty((int ChartId, int _Difficulty) key, bool selectLower = true)
        {
            return FindManyClosestCore(secondaryIndex0, secondaryIndex0Selector, System.Collections.Generic.Comparer<(int ChartId, int _Difficulty)>.Default, key, selectLower);
        }

        public RangeView<RankItem> FindRangeByChartIdAnd_Difficulty((int ChartId, int _Difficulty) min, (int ChartId, int _Difficulty) max, bool ascendant = true)
        {
            return FindManyRangeCore(secondaryIndex0, secondaryIndex0Selector, System.Collections.Generic.Comparer<(int ChartId, int _Difficulty)>.Default, min, max, ascendant);
        }

        public RangeView<RankItem> FindByMusicId(int key)
        {
            return FindManyCore(secondaryIndex1, secondaryIndex1Selector, System.Collections.Generic.Comparer<int>.Default, key);
        }

        public RangeView<RankItem> FindClosestByMusicId(int key, bool selectLower = true)
        {
            return FindManyClosestCore(secondaryIndex1, secondaryIndex1Selector, System.Collections.Generic.Comparer<int>.Default, key, selectLower);
        }

        public RangeView<RankItem> FindRangeByMusicId(int min, int max, bool ascendant = true)
        {
            return FindManyRangeCore(secondaryIndex1, secondaryIndex1Selector, System.Collections.Generic.Comparer<int>.Default, min, max, ascendant);
        }

        public RangeView<RankItem> FindByChartHash(string key)
        {
            return FindManyCore(secondaryIndex2, secondaryIndex2Selector, System.StringComparer.Ordinal, key);
        }

        public RangeView<RankItem> FindClosestByChartHash(string key, bool selectLower = true)
        {
            return FindManyClosestCore(secondaryIndex2, secondaryIndex2Selector, System.StringComparer.Ordinal, key, selectLower);
        }

        public RangeView<RankItem> FindRangeByChartHash(string min, string max, bool ascendant = true)
        {
            return FindManyRangeCore(secondaryIndex2, secondaryIndex2Selector, System.StringComparer.Ordinal, min, max, ascendant);
        }

        public RangeView<RankItem> FindByCreatedAt(DateTime key)
        {
            return FindManyCore(secondaryIndex3, secondaryIndex3Selector, System.Collections.Generic.Comparer<DateTime>.Default, key);
        }

        public RangeView<RankItem> FindClosestByCreatedAt(DateTime key, bool selectLower = true)
        {
            return FindManyClosestCore(secondaryIndex3, secondaryIndex3Selector, System.Collections.Generic.Comparer<DateTime>.Default, key, selectLower);
        }

        public RangeView<RankItem> FindRangeByCreatedAt(DateTime min, DateTime max, bool ascendant = true)
        {
            return FindManyRangeCore(secondaryIndex3, secondaryIndex3Selector, System.Collections.Generic.Comparer<DateTime>.Default, min, max, ascendant);
        }


        void ITableUniqueValidate.ValidateUnique(ValidateResult resultSet)
        {
            ValidateUniqueCore(data, primaryIndexSelector, "Id", resultSet);       
        }

        public static MasterMemory.Meta.MetaTable CreateMetaTable()
        {
            return new MasterMemory.Meta.MetaTable(typeof(RankItem), typeof(RankItemTable), "rank",
                new MasterMemory.Meta.MetaProperty[]
                {
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("Id")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("ChartId")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("_Difficulty")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("MusicId")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("Judge")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("Acc")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("Combo")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("Score")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("Mods")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("ChartHash")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("ReplayFile")),
                    new MasterMemory.Meta.MetaProperty(typeof(RankItem).GetProperty("CreatedAt")),
                },
                new MasterMemory.Meta.MetaIndex[]{
                    new MasterMemory.Meta.MetaIndex(new System.Reflection.PropertyInfo[] {
                        typeof(RankItem).GetProperty("Id"),
                    }, true, true, System.Collections.Generic.Comparer<int>.Default),
                    new MasterMemory.Meta.MetaIndex(new System.Reflection.PropertyInfo[] {
                        typeof(RankItem).GetProperty("ChartId"),
                        typeof(RankItem).GetProperty("_Difficulty"),
                    }, false, false, System.Collections.Generic.Comparer<(int ChartId, int _Difficulty)>.Default),
                    new MasterMemory.Meta.MetaIndex(new System.Reflection.PropertyInfo[] {
                        typeof(RankItem).GetProperty("MusicId"),
                    }, false, false, System.Collections.Generic.Comparer<int>.Default),
                    new MasterMemory.Meta.MetaIndex(new System.Reflection.PropertyInfo[] {
                        typeof(RankItem).GetProperty("ChartHash"),
                    }, false, false, System.StringComparer.Ordinal),
                    new MasterMemory.Meta.MetaIndex(new System.Reflection.PropertyInfo[] {
                        typeof(RankItem).GetProperty("CreatedAt"),
                    }, false, false, System.Collections.Generic.Comparer<DateTime>.Default),
                });
        }

    }
}
