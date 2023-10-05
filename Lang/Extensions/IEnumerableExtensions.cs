using System.Diagnostics;
using System.Threading.Tasks;

namespace Common.Lang.Extensions
{
    [DebuggerStepThrough]
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Return true if no items match the condition; false otherwise.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="filter"></param>
        /// <returns></returns>

        public static bool None<T>(this IEnumerable<T> self, Func<T, bool> filter)
        {
            return !self.Any(it => filter(it));
        }

        /// <summary>
        /// Aggregate into a single item T. Possibly a List or Dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="aggregator"></param>
        /// <returns></returns>
        public static T? Collect<T>(this IEnumerable<T> self, Func<T, T, T> aggregator)
        {
            var enumerable = self.ToList();
            return enumerable.Any() ? enumerable.Aggregate(aggregator) : default;
        }

        /// <summary>
        /// Shorthand of foreach () { }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var t in self)
            {
                action(t);
            }
        }

        /// <summary>
        /// Functional method of string.Join
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        public static string JoinString(this IEnumerable<string> self, string sep = ",")
        {
            return string.Join(sep, self);
        }

        /// <summary>
        /// Return list of items of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<T> WithType<T>(this IEnumerable<object> self) where T : class
        {
            return self.Where(it => it is T).Select(it => it.UnsafeCast<T>());
        }

        /// <summary>
        /// Return list of item with indexes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }

        public static T? FirstOrNull<T>(this IEnumerable<T> self) where T : struct
        {
            try
            {
                return self.First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        //  https://stackoverflow.com/questions/3575925/linq-to-return-all-pairs-of-elements-from-two-lists
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
        }

        public static IEnumerable<T> OnNext<T>(this IEnumerable<T> self, Action<T> onNext)
        {
            foreach (var item in self)
            {
                onNext(item);
                yield return item;
            }
        }
        
        public static IDictionary<K, V> GroupToDictionary<T, K, V>(this IEnumerable<T> self, Func<T, K> keySelector, Func<IEnumerable<T>, V> valueSelector) where K : notnull {
            var values = self.GroupBy(keySelector)
                            .Select(x => new KeyValuePair<K, V>(x.Key, valueSelector(x)));
            return new Dictionary<K, V>(values);
        }
    }
}