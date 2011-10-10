﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CassandraClient.Abstractions;

using NUnit.Framework;

namespace Tests.Tests
{
    public class ConnectionPoolTest : CassandraFunctionalTestWithRemoveKeyspacesBase
    {
        [Test]
        public void Test()
        {
            var threads = new List<Thread>();
            for (int i = 0; i < 100; i++)
            {
                int i1 = i;
                var thread = new Thread(() => FillColumnFamily(i1));
                threads.Add(thread);
                thread.Start();
            }
            int maxFree = 0;
            int maxBusy = 0;
            while (true)
            {
                Thread.Sleep(5000);
                var know = cassandraCluster.GetKnowledges();
                Console.WriteLine("-------------------------------");
                Console.WriteLine(know.Count);
                foreach(var kvp in know)
                {
                    Console.WriteLine(kvp.Key.IpEndPoint + " " + kvp.Key.Keyspace + " " + kvp.Value.BusyConnectionCount + " " + kvp.Value.FreeConnectionCount);
                    maxBusy = Math.Max(maxBusy, kvp.Value.BusyConnectionCount);
                    maxFree = Math.Max(maxFree, kvp.Value.FreeConnectionCount);
                    Assert.IsTrue(kvp.Value.BusyConnectionCount < 100);
                    Assert.IsTrue(kvp.Value.FreeConnectionCount < 100);
                }

                var flag = threads.Aggregate(false, (current, thread) => current || (thread.IsAlive));
                if (!flag) break;
            }
            Console.WriteLine(string.Format("Max free = {0}; Max busy: {1}", maxFree, maxBusy));
        }

        private void FillColumnFamily(int id)
        {
            for(int i = 0; i < 100; i++)
            {
                using(var connection = cassandraCluster.RetrieveColumnFamilyConnection(Constants.KeyspaceName, Constants.ColumnFamilyName))
                {
                    var list = new List<Column>();
                    for(int j = 0; j < 1000; j++)
                    {
                        list.Add(new Column
                            {
                                Name = string.Format("name_{0}_{1}_{2}", id, i, j),
                                Value = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30}
                            });
                    }
                    connection.AddBatch(string.Format("row_{0}_{1}", id, i), list);
                }
            }
        }
    }
}