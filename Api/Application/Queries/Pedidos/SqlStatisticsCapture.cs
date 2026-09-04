using System.Diagnostics;
using Api.Application.Models.Pedidos;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Queries.Pedidos;

/// <summary>
/// Mede tempo de execução (Stopwatch) e logical reads (via sys.dm_exec_query_stats) de uma consulta.
///
/// LIMITAÇÃO CONHECIDA: SqlConnection.InfoMessage não expõe as mensagens de "SET STATISTICS IO"
/// (limitação do driver ADO.NET), então os logical reads vêm do plan cache do SQL Server em vez disso.
/// Esse método soma o "last_logical_reads" de TODAS as queries executadas no servidor desde o timestamp
/// inicial — não isola só a consulta que estamos medindo. Em ambiente de teste isolado (sem concorrência)
/// costuma bater, mas os números ficam inconsistentes/ruidosos sob qualquer outra atividade no servidor,
/// e uma query cujo plano já estava em cache pode não gerar uma nova entrada de stats na janela medida
/// (aparecendo como 0 reads mesmo tendo rodado). O valor de ExecutionTimeMs (Stopwatch) é confiável;
/// LogicalReads deste endpoint deve ser tratado como aproximação, não como medição precisa.
///
/// Para número de I/O confiável, o caminho certo é medir manualmente: rodar a query isolada com
/// SET STATISTICS IO ON num cliente SQL (sqlcmd/SSMS/Azure Data Studio) — foi assim que os números
/// documentados em docs/benchmarks.md foram coletados (via database/scripts/03 e 04).
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
