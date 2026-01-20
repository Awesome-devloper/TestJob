using Microsoft.Extensions.Hosting;

public class BackgroundWorkerService : IHostedService
{
    private readonly BackgroundWorker _worker;
    private Task? _runningTask;
    private readonly CancellationTokenSource _cts = new();

    public BackgroundWorkerService(BackgroundWorker worker)
    {
        _worker = worker;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _runningTask = _worker.RunAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        if (_runningTask != null)
        {
            await _runningTask.WaitAsync(cancellationToken);
        }
    }
}