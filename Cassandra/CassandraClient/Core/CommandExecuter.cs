﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;

using log4net;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Core.GenericPool;
using SKBKontur.Cassandra.CassandraClient.Exceptions;

namespace SKBKontur.Cassandra.CassandraClient.Core
{
    using IThriftConnectionReplicaSetPool = IPoolSet<IThriftConnection, string>;

    internal class CommandExecuter : ICommandExecuter
    {
        public CommandExecuter(
            IThriftConnectionReplicaSetPool dataCommandsConnectionPool,
            IThriftConnectionReplicaSetPool fierceCommandsConnectionPool,
            ICassandraClusterSettings settings)
        {
            this.dataCommandsConnectionPool = dataCommandsConnectionPool;
            this.fierceCommandsConnectionPool = fierceCommandsConnectionPool;
            this.settings = settings;
        }

        public void Execute(Func<int, ICommand> createCommand)
        {
            var stopwatch = Stopwatch.StartNew();
            var command = createCommand(0);
            var pool = command.IsFierce ? fierceCommandsConnectionPool : dataCommandsConnectionPool;
            try
            {
                for(var i = 0; i < settings.Attempts; ++i)
                {
                    IThriftConnection connectionInPool = null;
                    try
                    {
                        connectionInPool = pool.Acquire(command.CommandContext.KeyspaceName);
                        connectionInPool.ExecuteCommand(command);
                        pool.Good(connectionInPool);
                        pool.Release(connectionInPool);
                        return;
                    }
                    catch(Exception e)
                    {
                        var message = string.Format("An error occurred while executing cassandra command '{0}'", command.Name);

                        var exception = CassandraExceptionTransformer.Transform(e, message);

                        if(connectionInPool != null)
                            logger.Warn(string.Format("Attempt {0} on {1} failed.", i, connectionInPool), exception);
                        else
                            logger.Warn(string.Format("Attempt {0} to all nodes failed.", i), exception);

                        if(connectionInPool != null)
                        {
                            if(exception.ReduceReplicaLive)
                                pool.Bad(connectionInPool);
                            else
                                pool.Good(connectionInPool);

                            if(exception.IsCorruptConnection)
                                pool.Remove(connectionInPool);
                            else
                                pool.Release(connectionInPool);
                        }
                        if(!exception.UseAttempts)
                            throw exception;
                        command = createCommand(i + 1);
                        if(i + 1 == settings.Attempts)
                            throw new CassandraAttemptsException(settings.Attempts, exception);
                    }
                }
            }
            finally
            {
                var timeStatisticsTitle = string.Format("Cassandra.{0}{1}", command.Name, command.CommandContext);
                var timeStatistics = timeStatisticsDictionary.GetOrAdd(timeStatisticsTitle, x => new TimeStatistics(timeStatisticsTitle));
                timeStatistics.AddTime(stopwatch.ElapsedMilliseconds);
            }
        }

        public void Execute(ICommand command)
        {
            Execute(attempt => command);
        }

        public void Dispose()
        {
            if(!disposed)
            {
                lock(disposeLock)
                {
                    if(!disposed)
                    {
                        disposed = true;
                        dataCommandsConnectionPool.Dispose();
                        fierceCommandsConnectionPool.Dispose();
                    }
                }
            }
        }

        private volatile bool disposed;
        private readonly object disposeLock = new object();
        private readonly ConcurrentDictionary<string, TimeStatistics> timeStatisticsDictionary = new ConcurrentDictionary<string, TimeStatistics>();
        private readonly IThriftConnectionReplicaSetPool dataCommandsConnectionPool;
        private readonly IThriftConnectionReplicaSetPool fierceCommandsConnectionPool;
        private readonly ICassandraClusterSettings settings;
        private readonly ILog logger = LogManager.GetLogger(typeof(CommandExecuter));
    }
}