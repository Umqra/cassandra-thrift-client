﻿using System.Net;

using NUnit.Framework;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Scheme;
using SKBKontur.Cassandra.ClusterDeployment;
using SKBKontur.Cassandra.FunctionalTests.Tests.SchemaTests.Spies;

namespace SKBKontur.Cassandra.FunctionalTests.Tests.SchemaTests
{
    [TestFixture]
    public class ActualizeKeyspaceTest
    {
        [SetUp]
        public void SetUp()
        {
            cluster = new CassandraClusterSpy(() => new CassandraCluster(StartSingleCassandraSetUp.Node.CreateSettings(IPAddress.Loopback)));
            actualize = new SchemeActualizer(cluster, null);
        }

        [TearDown]
        public void TearDown()
        {
            cluster.Dispose();
        }

        [Test]
        public void TestDoubleActualizeWithoutChangingSchema()
        {
            var scheme = new KeyspaceScheme
                {
                    Name = TestSchemaUtils.GetRandomKeyspaceName(),
                    Configuration = new KeyspaceConfiguration
                        {
                            ColumnFamilies = new[]
                                {
                                    new ColumnFamily
                                        {
                                            Name = "CF1"
                                        }
                                }
                        }
                };
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(0));
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(0));
        }

        [Test]
        public void TestChangeCompressionProperty()
        {
            var scheme = new KeyspaceScheme
                {
                    Name = TestSchemaUtils.GetRandomKeyspaceName(),
                    Configuration = new KeyspaceConfiguration
                        {
                            ColumnFamilies = new[]
                                {
                                    new ColumnFamily
                                        {
                                            Name = "CF1"
                                        }
                                }
                        }
                };
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(0));
            scheme.Configuration.ColumnFamilies[0].Compression = ColumnFamilyCompression.Deflate(new CompressionOptions {ChunkLengthInKb = 1024});
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(1));
        }

        [Test]
        public void TestChangeCompactionProperties()
        {
            var scheme = new KeyspaceScheme
                {
                    Name = TestSchemaUtils.GetRandomKeyspaceName(),
                    Configuration = new KeyspaceConfiguration
                        {
                            ColumnFamilies = new[]
                                {
                                    new ColumnFamily
                                        {
                                            Name = "CF1"
                                        }
                                }
                        }
                };
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(0));

            scheme.Configuration.ColumnFamilies[0].CompactionStrategy = CompactionStrategy.SizeTieredCompactionStrategy(4, 32);
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(0));

            scheme.Configuration.ColumnFamilies[0].CompactionStrategy = CompactionStrategy.SizeTieredCompactionStrategy(3, 32);
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(1));

            scheme.Configuration.ColumnFamilies[0].CompactionStrategy = CompactionStrategy.SizeTieredCompactionStrategy(3, 31);
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(2));

            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(2));

            scheme.Configuration.ColumnFamilies[0].CompactionStrategy = CompactionStrategy.LeveledCompactionStrategy(new CompactionStrategyOptions {SstableSizeInMb = 10});
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(3));

            scheme.Configuration.ColumnFamilies[0].CompactionStrategy = CompactionStrategy.LeveledCompactionStrategy(new CompactionStrategyOptions {SstableSizeInMb = 11});
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(4));

            scheme.Configuration.ColumnFamilies[0].CompactionStrategy = CompactionStrategy.LeveledCompactionStrategy(new CompactionStrategyOptions {SstableSizeInMb = 11}, 4, 32);
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(4));

            scheme.Configuration.ColumnFamilies[0].CompactionStrategy = CompactionStrategy.LeveledCompactionStrategy(new CompactionStrategyOptions {SstableSizeInMb = 11}, 3, 32);
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(5));
            
            scheme.Configuration.ColumnFamilies[0].CompactionStrategy = CompactionStrategy.LeveledCompactionStrategy(new CompactionStrategyOptions {SstableSizeInMb = 11}, 3, 31);
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(6));
            
            scheme.Configuration.ColumnFamilies[0].CompactionStrategy = CompactionStrategy.LeveledCompactionStrategy(new CompactionStrategyOptions {SstableSizeInMb = 11}, 3, 31);
            actualize.ActualizeKeyspaces(new[] {scheme});
            Assert.That(cluster.UpdateColumnFamilyInvokeCount, Is.EqualTo(6));
        }

        [Test]
        public void TestActualizeWithNullProperties()
        {
            var keyspaceName = TestSchemaUtils.GetRandomKeyspaceName();
            var scheme = new KeyspaceScheme
                {
                    Name = keyspaceName,
                    Configuration = new KeyspaceConfiguration
                        {
                            ColumnFamilies = new[]
                                {
                                    new ColumnFamily
                                        {
                                            Name = "CF1",
                                            Compression = ColumnFamilyCompression.Deflate(new CompressionOptions {ChunkLengthInKb = 1024}),
                                            Caching = ColumnFamilyCaching.KeysOnly
                                        }
                                }
                        }
                };
            actualize.ActualizeKeyspaces(new[] {scheme});

            var actualScheme = cluster.RetrieveKeyspaceConnection(keyspaceName).DescribeKeyspace();
            Assert.That(actualScheme.ColumnFamilies["CF1"].Compression.Algorithm, Is.EqualTo(CompressionAlgorithms.Deflate));

            scheme.Configuration.ColumnFamilies[0].Compression = null;
            scheme.Configuration.ColumnFamilies[0].Caching = ColumnFamilyCaching.All;
            actualize.ActualizeKeyspaces(new[] {scheme});

            actualScheme = cluster.RetrieveKeyspaceConnection(keyspaceName).DescribeKeyspace();
            Assert.That(actualScheme.ColumnFamilies["CF1"].Compression.Algorithm, Is.EqualTo(CompressionAlgorithms.Snappy));
        }
        
        [Test]
        public void TestUpdateColumnFamilyCaching()
        {
            var name = TestSchemaUtils.GetRandomColumnFamilyName();
            var originalColumnFamily = new ColumnFamily
                {
                    Name = name,
                    CompactionStrategy = CompactionStrategy.LeveledCompactionStrategy(new CompactionStrategyOptions {SstableSizeInMb = 10}),
                    GCGraceSeconds = 123,
                    ReadRepairChance = 0.3,
                    Caching = ColumnFamilyCaching.All
                };
            var keyspaceName = TestSchemaUtils.GetRandomKeyspaceName();
            var keyspaceSchemes = new[]
                {
                    new KeyspaceScheme
                        {
                            Name = keyspaceName,
                            Configuration = new KeyspaceConfiguration
                                {
                                    ColumnFamilies = new[]
                                        {
                                            originalColumnFamily
                                        }
                                }
                        }
                };

            cluster.ActualizeKeyspaces(keyspaceSchemes);

            originalColumnFamily.Caching = ColumnFamilyCaching.None;
            cluster.ActualizeKeyspaces(keyspaceSchemes);
            Assert.That(cluster.RetrieveKeyspaceConnection(keyspaceName).DescribeKeyspace().ColumnFamilies[name].Caching, Is.EqualTo(ColumnFamilyCaching.None));

            originalColumnFamily.Caching = ColumnFamilyCaching.KeysOnly;
            cluster.ActualizeKeyspaces(keyspaceSchemes);
            Assert.That(cluster.RetrieveKeyspaceConnection(keyspaceName).DescribeKeyspace().ColumnFamilies[name].Caching, Is.EqualTo(ColumnFamilyCaching.KeysOnly));

            originalColumnFamily.Caching = ColumnFamilyCaching.RowsOnly;
            cluster.ActualizeKeyspaces(keyspaceSchemes);
            Assert.That(cluster.RetrieveKeyspaceConnection(keyspaceName).DescribeKeyspace().ColumnFamilies[name].Caching, Is.EqualTo(ColumnFamilyCaching.RowsOnly));

            originalColumnFamily.Caching = ColumnFamilyCaching.All;
            cluster.ActualizeKeyspaces(keyspaceSchemes);
            Assert.That(cluster.RetrieveKeyspaceConnection(keyspaceName).DescribeKeyspace().ColumnFamilies[name].Caching, Is.EqualTo(ColumnFamilyCaching.All));
        }

        private CassandraClusterSpy cluster;
        private SchemeActualizer actualize;
    }
}