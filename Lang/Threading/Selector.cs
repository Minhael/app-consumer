using System.Diagnostics;
using Common.Lang.Extensions;

namespace Common.Lang.Threading;

public static class Selector
{
    /// <summary>
    /// Select the first ready Task<T> and discard others with proper cancallation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    //  https://www.codeproject.com/Questions/5317244/How-do-I-keep-waiting-for-a-list-of-tasks-until-on
    //  https://stackoverflow.com/questions/39589640/ignore-the-tasks-throwing-exceptions-at-task-whenall-and-get-only-the-completed
    [DebuggerStepThrough]
    public static async Task<T> Select<T>(Func<CancellationToken, IEnumerable<Task<T>>> self, CancellationToken cancellationToken = default)
    {
        //  Wrap token and pass to factory
        using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var tasks = self(source.Token).ToList();

        //  Wrap task with maybe to avoid exceptions
        var maybes = tasks.Select(x => Maybe<T>.Wrap(x)).ToList();
        Task<T>? selected = null;
        var delayed = new List<Exception>();

        try
        {
            //  Execute selector
            while (!source.IsCancellationRequested && selected == null && maybes.Count > 0)
            {
                var completed = await Task.WhenAny(maybes);
                maybes.Remove(completed);
                var maybe = await completed;
                if (maybe.Exception != null)
                    delayed.Add(maybe.Exception);
                else
                    selected = maybe.Origin;
            }

            //  Return selected result or throw aggregated exceptions
            if (selected != null)
                return await selected;
            else
                throw new AggregateException(delayed);
        }
        finally
        {
            //  Cancel all tasks
            source.Cancel();
            try
            {
                await Task.WhenAll(maybes);
            }
            catch (TaskCanceledException)
            {
                //  Possible cancelled exceptions that can safely ignored
            }
            catch (OperationCanceledException)
            {
                //  Possible cancelled exceptions that can safely ignored
            }

            //  Dispose completed results
            var discarded = tasks.Where(x => x != selected && !x.IsCanceled && !x.IsFaulted)
                                 .MapNotNull(x => x.Result)
                                 .ToArray();
            await Task.WhenAll(discarded.Where(x => x is IAsyncDisposable)
                                        .Select(x => x!.UnsafeCast<IAsyncDisposable>().DisposeAsync().AsTask()));
            discarded.Where(x => x is IDisposable && x is not IAsyncDisposable)
                     .ForEach(x => x!.UnsafeCast<IDisposable>().Dispose());
        }
    }
}

class Maybe<T>
{
    public static Task<Maybe<T>> Wrap(Task<T> task)
    {
        return task.ContinueWith(t => t.IsFaulted ? new Maybe<T> { Origin = t, Exception = t.Exception } : new Maybe<T> { Origin = t });
    }

    public Task<T> Origin { get; init; } = Task.FromException<T>(new NotImplementedException());
    public Exception? Exception { get; init; }
}