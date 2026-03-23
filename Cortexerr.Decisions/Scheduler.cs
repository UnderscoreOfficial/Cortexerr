using System.Collections.Concurrent;
using Cortexerr.Core.Configuration;
using Cortexerr.Core.Ingest;
using Cortexerr.Decisions.Logic;
using Cortexerr.Extended.DataStructures;

namespace Cortexerr.Decisions.Orchestration;

public static class Scheduler
{
    private static readonly ConcurrentQueue<RequestJob> _queue = new();
    private static readonly SemaphoreSlim _semaphore =
        new(Config.ARGS.max_queued_jobs, Config.ARGS.max_queued_jobs);
    private static int _running = 0; // 0 false, 1 true. Can't use bool because compareexchange

    async private static Task RequestQueueSemaphoreProcess(DecisionLogic logic)
    {
        try
        {
            while (_queue.TryDequeue(out var request_job))
            {
                await _semaphore.WaitAsync();
                // intentionally ignoring the outer await to support multiple concurrent running request_jobs
                _ = Task.Run(async () =>
                {
                    try { await Sequence.Process(logic, request_job); }
                    finally { _semaphore.Release(); }
                });
            }
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
            if (!_queue.IsEmpty && Interlocked.CompareExchange(ref _running, 1, 0) == 0)
            {
                _ = Task.Run(() => RequestQueueSemaphoreProcess(logic));
            }
        }
    }

    public static void RequestQueueAdd(DecisionLogic logic, Ingest ingest)
    {
        var request_job = new RequestJob
        {
            ingest = ingest
        };
        _queue.Enqueue(request_job);

        if (Interlocked.CompareExchange(ref _running, 1, 0) == 0)
        {
            _ = Task.Run(() => RequestQueueSemaphoreProcess(logic));
        }
    }
}
