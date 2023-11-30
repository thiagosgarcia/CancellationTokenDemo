using System;
using Microsoft.Extensions.Logging;

namespace CancellationTokenDemo.Attributes;

public class InternalLogger : ILogger, IDisposable
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return this;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = string.Empty;
        message += formatter(state, exception);
        Console.WriteLine($"[{logLevel.ToString()}] - {message}");
    }

    public void Dispose()
    {
    }
}