﻿using System;
using System.Collections.Generic;
using System.Net;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Core.Pools;
using SKBKontur.Cassandra.CassandraClient.Exceptions;
using SKBKontur.Cassandra.CassandraClient.Log;

namespace SKBKontur.Cassandra.CassandraClient.Core
{
    public class CommandExecuter : ICommandExecuter
    {
        public CommandExecuter(IClusterConnectionPool clusterConnectionPool,
                               IEndpointManager endpointManager,
                               ICassandraClusterSettings settings,
                               ICassandraLogManager logManager)
        {
            this.clusterConnectionPool = clusterConnectionPool;
            this.endpointManager = endpointManager;
            this.settings = settings;
            this.logManager = logManager;
            logger = logManager.GetLogger(GetType());
            recognizer = new CassandraClientExceptionTypeRecognizer();
            foreach(var ipEndPoint in settings.Endpoints)
                this.endpointManager.Register(ipEndPoint);
            this.endpointManager.Register(settings.EndpointForFierceCommands);
        }

        public Dictionary<ConnectionPoolKey, KeyspaceConnectionPoolKnowledge> GetKnowledges()
        {
            return clusterConnectionPool.GetKnowledges();
        }

        public void Execute(ICommand command)
        {
            logger.Debug("Start executing {0} command.", command.GetType());
            ValidationResult validationResult = command.Validate(logManager.GetLogger(command.GetType()));
            if(validationResult.Status != ValidationStatus.Ok)
                throw new CassandraClientInvalidRequestException(validationResult.Message);
            for(int i = 0; i < settings.Attempts; ++i)
            {
                IPEndPoint endpoint = command.IsFierce ? settings.EndpointForFierceCommands : endpointManager.GetEndPoint();
                try
                {
                    using(var thriftConnection = clusterConnectionPool.BorrowConnection(new ConnectionPoolKey {IpEndPoint = endpoint, Keyspace = command.Keyspace}))
                        thriftConnection.ExecuteCommand(command, logManager.GetLogger(command.GetType()));
                    endpointManager.Good(endpoint);
                    return;
                }
                catch(Exception e)
                {
                    string message = string.Format("An error occured while executing cassandra command '{0}'", command.GetType());
                    var exception = CassandraExceptionTransformer.Transform(e, message);
                    logger.Warn("Attempt " + i + " on " + endpoint + " failed.", exception);
                    endpointManager.Bad(endpoint);
                    if(!recognizer.IsExceptionRetryable(exception))
                        throw exception;
                    if(i + 1 == settings.Attempts)
                        throw new CassandraAttemptsException(settings.Attempts, exception);
                }
            }
            logger.Debug("Executing {0} command complete.", command.GetType());
        }

        private readonly IClusterConnectionPool clusterConnectionPool;
        private readonly IEndpointManager endpointManager;
        private readonly ICassandraClusterSettings settings;
        private readonly ICassandraLogManager logManager;
        private readonly CassandraClientExceptionTypeRecognizer recognizer;
        private ICassandraLogger logger;
    }
}