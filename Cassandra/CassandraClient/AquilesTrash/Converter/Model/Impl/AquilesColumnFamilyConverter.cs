﻿using System;
using System.Collections.Generic;
using System.Linq;

using Apache.Cassandra;

using SKBKontur.Cassandra.CassandraClient.AquilesTrash.Model;
using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace SKBKontur.Cassandra.CassandraClient.AquilesTrash.Converter.Model.Impl
{
    public class AquilesColumnFamilyConverter : IThriftConverter<AquilesColumnFamily, CfDef>
    {
        public CfDef Transform(AquilesColumnFamily objectA)
        {
            var columnFamily = new CfDef
                {
                    Keyspace = objectA.Keyspace,
                    Name = objectA.Name,
                    Column_type = "Standard",
                    Comment = objectA.Comment,
                    Subcomparator_type = objectA.SubComparator,
                    Default_validation_class = objectA.DefaultValidationClass,
                    Id = objectA.Id,
                };
            if(!String.IsNullOrEmpty(objectA.Comparator))
                columnFamily.Comparator_type = objectA.Comparator;
            if(objectA.GCGraceSeconds.HasValue)
                columnFamily.Gc_grace_seconds = objectA.GCGraceSeconds.Value;
            if(objectA.KeyCachedSize.HasValue)
                columnFamily.Key_cache_size = objectA.KeyCachedSize.Value;
            if(objectA.ReadRepairChance.HasValue)
                columnFamily.Read_repair_chance = objectA.ReadRepairChance.Value;
            if(objectA.RowCacheSize.HasValue)
                columnFamily.Row_cache_size = objectA.RowCacheSize.Value;
            if(objectA.MinimumCompactationThreshold.HasValue)
                columnFamily.Min_compaction_threshold = objectA.MinimumCompactationThreshold.Value;
            if(objectA.MaximumCompactationThreshold.HasValue)
                columnFamily.Max_compaction_threshold = objectA.MaximumCompactationThreshold.Value;
            if(objectA.RowCacheSavePeriodInSeconds.HasValue)
                columnFamily.Row_cache_save_period_in_seconds = objectA.RowCacheSavePeriodInSeconds.Value;
            if(objectA.KeyCacheSavePeriodInSeconds.HasValue)
                columnFamily.Key_cache_save_period_in_seconds = objectA.KeyCacheSavePeriodInSeconds.Value;
            if(objectA.Columns != null)
            {
                var originColumns = objectA.Columns;
                var destinationColumns = new List<ColumnDef>(originColumns.Count);
                destinationColumns.AddRange(originColumns.Select(definition => definition.ToCassandraColumnDef()));
                columnFamily.Column_metadata = destinationColumns;
            }

            return columnFamily;
        }

        public AquilesColumnFamily Transform(CfDef objectB)
        {
            var columnFamily = new AquilesColumnFamily
                {
                    Name = objectB.Name,
                    Comment = objectB.Comment,
                    Comparator = objectB.Comparator_type,
                    GCGraceSeconds = objectB.Gc_grace_seconds,
                    KeyCachedSize = objectB.Key_cache_size,
                    Keyspace = objectB.Keyspace,
                    ReadRepairChance = objectB.Read_repair_chance,
                    RowCacheSize = objectB.Row_cache_size,
                    SubComparator = objectB.Subcomparator_type,
                    DefaultValidationClass = objectB.Default_validation_class,
                    Id = objectB.Id,
                    MinimumCompactationThreshold = objectB.Min_compaction_threshold,
                    MaximumCompactationThreshold = objectB.Max_compaction_threshold,
                    RowCacheSavePeriodInSeconds = objectB.Row_cache_save_period_in_seconds,
                    KeyCacheSavePeriodInSeconds = objectB.Key_cache_save_period_in_seconds,
                };
            if(objectB.Column_metadata != null)
            {
                var originColumns = objectB.Column_metadata;
                var destinationColumns = new List<IndexDefinition>(originColumns.Count);
                destinationColumns.AddRange(originColumns.Select(def => def.FromCassandraColumnDef()));
                columnFamily.Columns = destinationColumns;
            }

            return columnFamily;
        }
    }
}