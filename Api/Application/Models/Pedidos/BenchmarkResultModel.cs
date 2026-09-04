namespace Api.Application.Models.Pedidos;

public class BenchmarkResultModel
{
    public QueryMetricsModel SlowQuery { get; set; } = new();
    public QueryMetricsModel OptimizedQuery { get; set; } = new();
}
