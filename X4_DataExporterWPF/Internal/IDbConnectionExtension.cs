using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace X4_DataExporterWPF.Internal
{
    internal static class IDbConnectionExtension
    {
        internal static async Task ExecuteAsync<T>(this IDbConnection connection, string query, IAsyncEnumerable<T> asyncEnumerable)
        {
            await connection.ExecuteAsync(query, asyncEnumerable.ToEnumerable());
        }
    }
}
