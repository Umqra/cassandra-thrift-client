using SKBKontur.Cassandra.CassandraClient.AquilesTrash.Encoders;

namespace SKBKontur.Cassandra.CassandraClient.Abstractions
{
    public class IndexExpression
    {
        public string ColumnName { get; set; }
        public IndexOperator IndexOperator { get; set; }
        public byte[] Value { get; set; }
    }

    internal static class IndexExpressionExtensions
    {
        public static Apache.Cassandra.IndexExpression ToCassandraIndexExpression(this IndexExpression indexExpression)
        {
            return new Apache.Cassandra.IndexExpression
                {
                    Column_name = ByteEncoderHelper.UTF8Encoder.ToByteArray(indexExpression.ColumnName),
                    Value = indexExpression.Value,
                    Op = indexExpression.IndexOperator.ToCassandraIndexOperator()
                };
        }
    }
}