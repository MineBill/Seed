using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using NLog;
using ReactiveUI;

namespace Seed;

public class ReactiveUIExceptionHandler : IObserver<Exception>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void OnCompleted()
    {
        if (Debugger.IsAttached) Debugger.Break();
        RxApp.MainThreadScheduler.Schedule(() => throw new NotImplementedException());
    }

    public void OnError(Exception error)
    {
        if (Debugger.IsAttached) Debugger.Break();

        Logger.Error(error, "ReactiveUI Exception");

        RxApp.MainThreadScheduler.Schedule(() => throw error);
    }

    public void OnNext(Exception value)
    {
        if (Debugger.IsAttached) Debugger.Break();

        Logger.Error(value, "ReactiveUI Exception");
        RxApp.MainThreadScheduler.Schedule(() => throw value);
    }
}