using System.Diagnostics;
using Api.Application.Models.Pedidos;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Queries.Pedidos;

/// <summary>
/// Mede tempo de execução (Stopwatch) e logical reads reais (via sys.dm_exec_query_stats) de uma consulta.
/// Obs: SqlConnection.InfoMessage não expõe as mensagens de "SET STATISTICS IO" (limitação conhecida do
/// driver ADO.NET), por isso os logical reads são obtidos consultando o plan cache do SQL Server em vez disso.
/// </summary>
public static class SqlStatisticsCapture
{
    public static async Task<QueryMetricsModel> RunAsync(AppDbContext context, Func<Task> query)
    {
        var start = DateTime.UtcNow;

        var stopwatch = Stopwatch.StartNew();
        await query();
        stopwatch.Stop();

        var logicalReads = await context.Database
            .SqlQuery<long>($@"
                SELECT ISNULL(SUM(qs.last_logical_reads), 0) AS Value
                FROM sys.dm_exec_query_stats qs
                WHERE qs.last_execution_time >= {start}")
            .FirstAsync();

        return new QueryMetricsModel
        {
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            LogicalReads = logicalReads
        };
    }
}
