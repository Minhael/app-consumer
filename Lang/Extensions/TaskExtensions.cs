using System.Diagnostics;
using Common.LazyTask;

namespace Common.Lang.Extensions
{
    public static class TaskExtensions
    {


        /// <summary>
        /// Remedy non cancellable task. WARNING: The actual task is not actually cancelled. This just free you from blocking the thread.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// 
        //  https://stackoverflow.com/questions/28626575/can-i-cancel-streamreader-readlineasync-with-a-cancellationtoken
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            using var delayCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var waiting = Task.Delay(-1, delayCts.Token);
            var doing = task;
            await Task.WhenAny(waiting, doing);
            delayCts.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            return await doing;
        }

        /// <summary>
        /// Convert and return the converted type.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static async Task<T> Map<TS, T>(this Task<TS> self, Func<TS, T> transformer)
        {
            return transformer(await self);
        }

        /// <summary>
        /// Convert and return the converted type.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static async LazyTask<T> Map<TS, T>(this LazyTask<TS> self, Func<TS, T> mapper)
        {
            var s = await self;
            return mapper(s);
        }

        /// <summary>
        /// Cast returned type.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static async Task<T> As<TS, T>(this Task<TS> self) where TS : T
        {
            return await self;
        }

        /// <summary>
        /// Execute list of task in sequential order.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static async Task SequentialAll(this IEnumerable<Task> self, CancellationToken token = default)
        {
            foreach (var exe in self)
            {
                if (token.IsCancellationRequested)
                    break;
                await exe;
            }
        }

        /// <summary>
        /// Execute list of task in sequential order.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static async Task<IEnumerable<T>> SequentialAll<T>(this IEnumerable<Task<T>> self, CancellationToken token = default)
        {
            var rt = new List<T>();
            foreach (var exe in self)
            {
                if (token.IsCancellationRequested)
                    break;
                rt.Add(await exe);
            }
            return rt;
        }

        /// <summary>
        /// Create LazyTask from Task.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static async LazyTask<T> Lazy<T>(this Task<T> self)
        {
            return await self;
        }

        /// <summary>
        /// Functional method for try { } catch { }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static async Task<T> TryCatch<T>(this Task<T> self, Func<Exception, Task<T>> onError)
        {
            try
            {
                return await self;
            }
            catch (Exception e)
            {
                return await onError(e);
            }
        }

        /// <summary>
        /// Convert enumerable to array.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static Task<T[]> ToArray<S, T>(this Task<S> self) where S : IEnumerable<T> => self.Map(x => x.ToArray());

        /// <summary>
        /// Convert enumerable to array.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static Task<T[]> ToArray<T>(this Task<IEnumerable<T>> self) => self.ToArray<IEnumerable<T>, T>();
    }
}