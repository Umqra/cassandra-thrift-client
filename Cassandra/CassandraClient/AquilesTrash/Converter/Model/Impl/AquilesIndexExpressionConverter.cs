﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CassandraClient.AquilesTrash.Model;
using Apache.Cassandra;

using CassandraClient.AquilesTrash.Converter.Model;

namespace CassandraClient.AquilesTrash.Converter.Model.Impl
{
    /// <summary>
    /// Converter for AquilesIndexExpression
    /// </summary>
    public class AquilesIndexExpressionConverter : IThriftConverter<AquilesIndexExpression, IndexExpression>
    {

        #region IThriftConverter<AquilesIndexExpression,IndexExpression> Members
        /// <summary>
        /// Transform AquilesIndexExpression structure into IndexExpression
        /// </summary>
        /// <param name="objectA"></param>
        /// <returns></returns>
        public IndexExpression Transform(AquilesIndexExpression objectA)
        {
            IndexExpression indexExpression = new IndexExpression();
            indexExpression.Column_name = objectA.ColumnName;
            int tempValue = (int)objectA.IndexOperator;
            indexExpression.Op = (IndexOperator)tempValue;
            indexExpression.Value = objectA.Value;

            return indexExpression;
        }

        /// <summary>
        /// Transform IndexExpression structure into AquilesIndexExpression
        /// </summary>
        /// <param name="objectB"></param>
        /// <returns></returns>
        public AquilesIndexExpression Transform(IndexExpression objectB)
        {
            AquilesIndexExpression indexExpression = new AquilesIndexExpression();
            indexExpression.ColumnName = objectB.Column_name;
            int tempValue = (int)objectB.Op;
            indexExpression.IndexOperator = (AquilesIndexOperator) tempValue;
            indexExpression.Value = objectB.Value;

            return indexExpression;
        }

        #endregion
    }
}