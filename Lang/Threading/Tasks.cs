namespace Common.Lang.Threading;

//  https://stackoverflow.com/questions/17197699/awaiting-multiple-tasks-with-different-results
public static class Tasks
{
    public static async Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> t1, Task<T2> t2)
    {
        await Task.WhenAll(t1, t2);
        return (await t1, await t2);
    }

    public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> t1, Task<T2> t2, Task<T3> t3)
    {
        await Task.WhenAll(t1, t2, t3);
        return (await t1, await t2, await t3);
    }

    public static async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4)
    {
        await Task.WhenAll(t1, t2, t3, t4);
        return (await t1, await t2, await t3, await t4);
    }

    public static async Task<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5)
    {
        await Task.WhenAll(t1, t2, t3, t4, t5);
        return (await t1, await t2, await t3, await t4, await t5);
    }

    public static async Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6)
    {
        await Task.WhenAll(t1, t2, t3, t4, t5, t6);
        return (await t1, await t2, await t3, await t4, await t5, await t6);
    }

    public static async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6, Task<T7> t7)
    {
        await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7);
        return (await t1, await t2, await t3, await t4, await t5, await t6, await t7);
    }

    public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6, Task<T7> t7, Task<T8> t8)
    {
        await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8);
        return (await t1, await t2, await t3, await t4, await t5, await t6, await t7, await t8);
    }

    public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6, Task<T7> t7, Task<T8> t8, Task<T9> t9)
    {
        await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        return (await t1, await t2, await t3, await t4, await t5, await t6, await t7, await t8, await t9);
    }

    public static async Task<(T1[], T2[])> WhenAll<T1, T2>(IEnumerable<Task<T1>> t1, IEnumerable<Task<T2>> t2)
    {
        await Task.WhenAll(
            t1.Cast<Task>()
              .Concat(t2)
        );
        return (
            await Task.WhenAll(t1),
            await Task.WhenAll(t2)
        );
    }

    public static async Task<(T1[], T2[], T3[])> WhenAll<T1, T2, T3>(IEnumerable<Task<T1>> t1, IEnumerable<Task<T2>> t2, IEnumerable<Task<T3>> t3)
    {
        await Task.WhenAll(
            t1.Cast<Task>()
              .Concat(t2)
              .Concat(t3)
        );
        return (
            await Task.WhenAll(t1),
            await Task.WhenAll(t2),
            await Task.WhenAll(t3)
        );
    }

    public static async Task<(T1[], T2[], T3[], T4[])> WhenAll<T1, T2, T3, T4>(IEnumerable<Task<T1>> t1, IEnumerable<Task<T2>> t2, IEnumerable<Task<T3>> t3, IEnumerable<Task<T4>> t4)
    {
        await Task.WhenAll(
            t1.Cast<Task>()
              .Concat(t2)
              .Concat(t3)
              .Concat(t4)
        );
        return (
            await Task.WhenAll(t1),
            await Task.WhenAll(t2),
            await Task.WhenAll(t3),
            await Task.WhenAll(t4)
        );
    }

    public static async Task<(T1[], T2[], T3[], T4[], T5[])> WhenAll<T1, T2, T3, T4, T5>(IEnumerable<Task<T1>> t1, IEnumerable<Task<T2>> t2, IEnumerable<Task<T3>> t3, IEnumerable<Task<T4>> t4, IEnumerable<Task<T5>> t5)
    {
        await Task.WhenAll(
            t1.Cast<Task>()
              .Concat(t2)
              .Concat(t3)
              .Concat(t4)
              .Concat(t5)
        );
        return (
            await Task.WhenAll(t1),
            await Task.WhenAll(t2),
            await Task.WhenAll(t3),
            await Task.WhenAll(t4),
            await Task.WhenAll(t5)
        );
    }

    public static async Task<(T1[], T2[], T3[], T4[], T5[], T6[])> WhenAll<T1, T2, T3, T4, T5, T6>(IEnumerable<Task<T1>> t1, IEnumerable<Task<T2>> t2, IEnumerable<Task<T3>> t3, IEnumerable<Task<T4>> t4, IEnumerable<Task<T5>> t5, IEnumerable<Task<T6>> t6)
    {
        await Task.WhenAll(
            t1.Cast<Task>()
              .Concat(t2)
              .Concat(t3)
              .Concat(t4)
              .Concat(t5)
              .Concat(t6)
        );
        return (
            await Task.WhenAll(t1),
            await Task.WhenAll(t2),
            await Task.WhenAll(t3),
            await Task.WhenAll(t4),
            await Task.WhenAll(t5),
            await Task.WhenAll(t6)
        );
    }

    public static async Task<(T1[], T2[], T3[], T4[], T5[], T6[], T7[])> WhenAll<T1, T2, T3, T4, T5, T6, T7>(IEnumerable<Task<T1>> t1, IEnumerable<Task<T2>> t2, IEnumerable<Task<T3>> t3, IEnumerable<Task<T4>> t4, IEnumerable<Task<T5>> t5, IEnumerable<Task<T6>> t6, IEnumerable<Task<T7>> t7)
    {
        await Task.WhenAll(
            t1.Cast<Task>()
              .Concat(t2)
              .Concat(t3)
              .Concat(t4)
              .Concat(t5)
              .Concat(t6)
              .Concat(t7)
        );
        return (
            await Task.WhenAll(t1),
            await Task.WhenAll(t2),
            await Task.WhenAll(t3),
            await Task.WhenAll(t4),
            await Task.WhenAll(t5),
            await Task.WhenAll(t6),
            await Task.WhenAll(t7)
        );
    }

    public static async Task<(T1[], T2[], T3[], T4[], T5[], T6[], T7[], T8[])> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(IEnumerable<Task<T1>> t1, IEnumerable<Task<T2>> t2, IEnumerable<Task<T3>> t3, IEnumerable<Task<T4>> t4, IEnumerable<Task<T5>> t5, IEnumerable<Task<T6>> t6, IEnumerable<Task<T7>> t7, IEnumerable<Task<T8>> t8)
    {
        await Task.WhenAll(
            t1.Cast<Task>()
              .Concat(t2)
              .Concat(t3)
              .Concat(t4)
              .Concat(t5)
              .Concat(t6)
              .Concat(t7)
              .Concat(t8)
        );
        return (
            await Task.WhenAll(t1),
            await Task.WhenAll(t2),
            await Task.WhenAll(t3),
            await Task.WhenAll(t4),
            await Task.WhenAll(t5),
            await Task.WhenAll(t6),
            await Task.WhenAll(t7),
            await Task.WhenAll(t8)
        );
    }

    public static async Task<(T1[], T2[], T3[], T4[], T5[], T6[], T7[], T8[], T9[])> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IEnumerable<Task<T1>> t1, IEnumerable<Task<T2>> t2, IEnumerable<Task<T3>> t3, IEnumerable<Task<T4>> t4, IEnumerable<Task<T5>> t5, IEnumerable<Task<T6>> t6, IEnumerable<Task<T7>> t7, IEnumerable<Task<T8>> t8, IEnumerable<Task<T9>> t9)
    {
        await Task.WhenAll(
            t1.Cast<Task>()
              .Concat(t2)
              .Concat(t3)
              .Concat(t4)
              .Concat(t5)
              .Concat(t6)
              .Concat(t7)
              .Concat(t8)
              .Concat(t9)
        );
        return (
            await Task.WhenAll(t1),
            await Task.WhenAll(t2),
            await Task.WhenAll(t3),
            await Task.WhenAll(t4),
            await Task.WhenAll(t5),
            await Task.WhenAll(t6),
            await Task.WhenAll(t7),
            await Task.WhenAll(t8),
            await Task.WhenAll(t9)
        );
    }
}