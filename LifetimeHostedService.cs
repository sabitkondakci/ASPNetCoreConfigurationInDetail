using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class LifetimeHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifeTime;

    public LifetimeHostedService(ILogger<LifetimeHostedService> logger, IHostApplicationLifetime appLifeTime)
    {
        this._logger = logger;
        this._appLifeTime = appLifeTime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifeTime.ApplicationStarted.Register(OnStarted);
        _appLifeTime.ApplicationStopping.Register(OnStopping);
        _appLifeTime.ApplicationStopped.Register(OnStopped);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        _logger.LogInformation("#OnStarted has been called#");

        // Perform post-startup activities here
    }

    private void OnStopping()
    {
        _logger.LogInformation("#OnStopping has been called#");

        // Perform on-stopping activities here
    }

    private void OnStopped()
    {
        _logger.LogInformation("#OnStopped has been called#");

        // Perform post-stopped activities here
    }

}