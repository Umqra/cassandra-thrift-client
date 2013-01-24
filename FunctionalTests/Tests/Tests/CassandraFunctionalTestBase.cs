using System;
using System.Collections.Generic;
using System.Reflection;

using Cassandra.Tests;

using GroboContainer.Core;
using GroboContainer.Impl;

using NUnit.Framework;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.FunctionalTests.Logger;
using SKBKontur.Cassandra.FunctionalTests.Utils;

namespace SKBKontur.Cassandra.FunctionalTests.Tests
{
    public abstract class CassandraFunctionalTestBase : TestBase
    {
        public override void SetUp()
        {
            TestNameHolder.TestName = TestContext.CurrentContext.Test.Name;
            base.SetUp();
            var assemblies = new List<Assembly>(AssembliesLoader.Load()) {Assembly.GetExecutingAssembly()};
            var container = new Container(new ContainerConfiguration(assemblies));
            cassandraCluster = container.Get<ICassandraCluster>();
            ServiceUtils.ConfugureLog4Net(AppDomain.CurrentDomain.BaseDirectory);
            KeyspaceName = "TestKeyspace_" + Guid.NewGuid().ToString("N");
            if(!keyspacesDeleted)
            {
                new CassandraClient(cassandraCluster).RemoveAllKeyspaces();
                keyspacesDeleted = true;
            }
        }

        private static bool keyspacesDeleted;

        protected string KeyspaceName { get; private set; }

        protected ICassandraCluster cassandraCluster;
    }
}