using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public interface ILoggerService
{
	void Log(string agent, string path, string user, string ip);
}

public class DatabaseLoggerService : BackgroundService, ILoggerService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Channel<VisitorLog> logMessageQueue;
    private readonly IHostApplicationLifetime HostApplicationLifetime;
    private readonly int? _maxQueueSize;

    public DatabaseLoggerService(
        IServiceScopeFactory scopeFactory,
        IHostApplicationLifetime hostApplicationLifetime,
        IConfiguration settings)
    {
        logMessageQueue = Channel.CreateUnbounded<VisitorLog>();
        HostApplicationLifetime = hostApplicationLifetime;
        _scopeFactory = scopeFactory;

        _maxQueueSize = settings.GetValue<int?>("SnitzForums:VisitorTracking");
    }

    public void Log(string agent, string path, string user, string? ip)
    {
        var logMessage = new VisitorLog
        {
            //Id = Thread.CurrentThread.ManagedThreadId,
            UserAgent = agent,
            Path = path,
            UserName = user,
            IpAddress = ip,
            VisitTime = DateTime.UtcNow
        };

        if (!logMessageQueue.Writer.TryWrite(logMessage))
        {
            throw new InvalidOperationException("Failed to enqueue log message.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var logRepository = scope.ServiceProvider.GetRequiredService<ILogRepository>();
                try
                {
                    var batch = await GetBatchAsync(stoppingToken);
                    await logRepository.InsertAsync(batch);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    HostApplicationLifetime.StopApplication();
                    return;
                }
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logMessageQueue.Writer.Complete();

        var remainingMessages = new List<VisitorLog>();
        while (logMessageQueue.Reader.TryRead(out var message))
        {
            remainingMessages.Add(message);
        }

        if (remainingMessages.Count > 0)
        {
            ////Console.WriteLine("Processing remaining log messages before shutdown...");
            using (var scope = _scopeFactory.CreateScope())
            {
                var logRepository = scope.ServiceProvider.GetRequiredService<ILogRepository>();
                try
                {
                    await logRepository.InsertAsync(remainingMessages);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    HostApplicationLifetime.StopApplication();
                    return;
                }
            }
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task<List<VisitorLog>> GetBatchAsync(CancellationToken cancellationToken)
    {
        var batch = new List<VisitorLog>();
        var firstItem = await logMessageQueue.Reader.ReadAsync(cancellationToken);
        batch.Add(firstItem);

        var flushTimeout = TimeSpan.FromSeconds(5);
        var flushCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var flushTask = Task.Delay(flushTimeout, flushCts.Token);
        try
        {
            while (batch.Count < _maxQueueSize)
            {
                var readTask = logMessageQueue.Reader.WaitToReadAsync(cancellationToken).AsTask();
                var completedTask = await Task.WhenAny(readTask, flushTask);

                if (completedTask == flushTask)
                    break; // Timeout reached, return what we have

                while (batch.Count < _maxQueueSize && logMessageQueue.Reader.TryRead(out var message))
                {
                    batch.Add(message);
                }
            }
        }
        finally
        {
            flushCts.Cancel();
            flushCts.Dispose();
        }

        ////Console.WriteLine($"Flushed batch of {batch.Count} logs at {DateTime.UtcNow}");
        ////Console.WriteLine("returning batch of " + batch.Count);
        return batch;
    }

}

