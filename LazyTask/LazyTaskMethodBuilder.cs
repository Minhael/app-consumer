using System.Runtime.CompilerServices;

namespace Common.LazyTask
{
    public class LazyTaskMethodBuilder<T>
    {
        public LazyTaskMethodBuilder() => Task = new LazyTask<T>();

        public static LazyTaskMethodBuilder<T> Create() => new();

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            Task.SetStateMachine(stateMachine);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            //  Lazy state cannot be reverted or reset
        }

        public void SetException(Exception exception) => Task.SetException(exception);

        public void SetResult(T result) => Task.SetResult(result);

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
            =>
                GenericAwaitOnCompleted(ref awaiter, ref stateMachine);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
            =>
                GenericAwaitOnCompleted(ref awaiter, ref stateMachine);

        public void GenericAwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public LazyTask<T> Task { get; }
    }
}