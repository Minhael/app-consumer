using System.Diagnostics;

namespace Common.Lang.Extensions
{
    [DebuggerStepThrough]
    public static class LinqExtensions
    {

        /// <summary>
        /// Apply items to a state and emit the changes of the state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TU"></typeparam>
        /// <param name="input"></param>
        /// <param name="state"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        //  https://stackoverflow.com/questions/6551271/summing-the-previous-values-in-an-ienumerable
        public static IEnumerable<TU> Scan<T, TU>(this IEnumerable<T> input, TU state, Func<TU, T, TU> next)
        {
            yield return state;
            foreach (var item in input)
            {
                state = next(state, item);
                yield return state;
            }
        }

        /// <summary>
        /// Merge list of lists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> self)
        {
            return self.SelectMany(i => i);
        }

        /// <summary>
        /// Map the value to another type nad drop null values.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> MapNotNull<TS, T>(this IEnumerable<TS> self, Func<TS, T?> action)
        {
            return self.Select(it => action(it)).Where(it => it != null).Select(it => it!);
        }

        /// <summary>
        /// Drop all null values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : class
        {
            return enumerable.Where(e => e != null).Select(e => e!);
        }

        /// <summary>
        /// Apply action to each item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IEnumerable<T> Apply<T>(this IEnumerable<T> self, Action<T> action)
        {
            return self.Select(x => x.Also(it => action(it)));
        }

#if !NET6_0_OR_GREATER
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> self, int size)
        {
            var enumerable = self.ToList();
            if (!enumerable.Any()) yield break;
            var total = (enumerable.Count - 1) / size + 1;
            for (var i = 0; i < total; ++i)
            {
                yield return enumerable.Skip(i * size).Take(size);
            }
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> self, Func<T, TKey> selector)
        {
            return self.GroupBy(selector).Select(x => x.First());
        }
#endif
    }
}