using System;
using System.Text;

namespace SKBKontur.Cassandra.CassandraClient.Helpers
{
    public static class StringHelpers
    {
        [Obsolete("��� ���������� ����� CassandraClient'�. ���������� ���� ���������� ������������ ������")]
        public static string BytesToString(byte[] bytes)
        {
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }

        [Obsolete("��� ���������� ����� CassandraClient'�. ���������� ���� ���������� ������������ ������")]
        public static byte[] StringToBytes(string str)
        {
            return str == null ? null : Encoding.UTF8.GetBytes(str);
        }
    }
}