﻿using System.Collections.Generic;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Abstractions.Internal;
using SKBKontur.Cassandra.CassandraClient.Commands.Base;

using ConsistencyLevel = Apache.Cassandra.ConsistencyLevel;

namespace SKBKontur.Cassandra.CassandraClient.Commands.Simple.Read
{
    internal class MultiGetCountCommand : KeyspaceColumnFamilyDependantCommandBase, ISimpleCommand
    {
        public MultiGetCountCommand(string keyspace, string columnFamily, ConsistencyLevel consistencyLevel, List<byte[]> keys, SlicePredicate predicate)
            : base(keyspace, columnFamily)
        {
            this.consistencyLevel = consistencyLevel;
            this.keys = keys;
            this.predicate = predicate ?? new SlicePredicate(new SliceRange {Count = int.MaxValue});
        }

        public override void Execute(Apache.Cassandra.Cassandra.Client cassandraClient)
        {
            Output = cassandraClient.multiget_count(keys, BuildColumnParent(), predicate.ToCassandraSlicePredicate(), consistencyLevel);
        }

        public Dictionary<byte[], int> Output { get; private set; }
        public int QueriedPartitionsCount => keys.Count;

        private readonly ConsistencyLevel consistencyLevel;
        private readonly List<byte[]> keys;
        private readonly SlicePredicate predicate;
    }
}