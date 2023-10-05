using System.Diagnostics;
using System.Linq.Expressions;

namespace Common.Lang.Extensions
{
    [DebuggerStepThrough]
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Return the value of the key if key exists; otherwise set the value and return the default value.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static T GetOrPut<TS, T>(this IDictionary<TS, T> self, TS key, Func<T> defValue) where TS : notnull
        {
            if (!self.ContainsKey(key))
                self[key] = defValue();
            return self[key];
        }

        /// <summary>
        /// Return the value of the key if key exists; otherwise return default value.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns>value of the key if key exists; othersie default value</returns>
        public static T? GetOrDefault<TS, T>(this Dictionary<TS, T> self, TS key) where TS : notnull
        {
            return self.ContainsKey(key) ? self[key] : default;
        }

        /// <summary>
        /// Return the value of the key if key exists; otherwise return default value.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T? GetOrDefault<TS, T>(this IDictionary<TS, T> self, TS key) where TS : notnull
        {
            if (self.ContainsKey(key))
                return self[key];
            return default;
        }

        /// <summary>
        /// Return the value of the key if key exists; otherwise return default value.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T? GetOrDefault<TS, T>(this IReadOnlyDictionary<TS, T> self, TS key) where TS : notnull
        {
            if (self.ContainsKey(key))
                return self[key];
            return default;
        }

        /// <summary>
        /// Convert value type from TU to TV
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TU"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="self"></param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        public static IDictionary<T, TV> Map<T, TU, TV>(this IDictionary<T, TU> self, Func<KeyValuePair<T, TU>, KeyValuePair<T, TV>> transformer) where T : notnull
        {
            return self.Select(transformer).ToDictionary(it => it.Key, it => it.Value);
        }

        /// <summary>
        /// Convert value type from TU to TV
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TU"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="self"></param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        public static IDictionary<T, TV> Map<T, TU, TV>(this Dictionary<T, TU> self, Func<TU, TV> transformer) where T : notnull
        {
            return self.Select(it => new KeyValuePair<T, TV>(it.Key, transformer(it.Value))).ToDictionary(it => it.Key, it => it.Value);
        }

        /// <summary>
        /// Convert value type from TU to TV
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TU"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="self"></param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        public static IDictionary<T, TV> Map<T, TU, TV>(this IReadOnlyDictionary<T, TU> self, Func<TU, TV> transformer) where T : notnull
        {
            return self.Select(it => new KeyValuePair<T, TV>(it.Key, transformer(it.Value))).ToDictionary(it => it.Key, it => it.Value);
        }

        /// <summary>
        /// Convert value type from TU to TV
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TU"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="self"></param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        public static IDictionary<T, TV> Map<T, TU, TV>(this IDictionary<T, TU> self, Func<TU, TV> transformer) where T : notnull
        {
            return self.Select(it => new KeyValuePair<T, TV>(it.Key, transformer(it.Value))).ToDictionary(it => it.Key, it => it.Value);
        }

        /// <summary>
        /// Convert value type from TU to TV. Drop the key if the converted value is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TU"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="self"></param>
        /// <param name="transformer"></param>
        /// <returns></returns>
        public static IDictionary<T, TV> MapNotNull<T, TU, TV>(this IDictionary<T, TU> self, Func<TU, TV?> transformer) where T : notnull
        {
            return self
                .Select(it => new { it.Key, Value = transformer(it.Value) })
                .Where(it => it.Value != null)
                .Select(it => new KeyValuePair<T, TV>(it.Key, it.Value!))
                .ToDictionary(it => it.Key, it => it.Value);
        }

        public delegate TV Remapper<in T, TV>(T key, TV left, TV right);

        /// <summary>
        /// Union dictionaries. When in conflicts, by default incoming values take priority.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <param name="remapper"></param>
        public static void Put<T, TV>(this IDictionary<T, TV> self, IDictionary<T, TV>? other, Remapper<T, TV>? remapper = null) where T : notnull
        {
            if (remapper == null)
                remapper = TakeRight;

            if (other == null)
                return;

            foreach (KeyValuePair<T, TV> item in other)
            {
                self[item.Key] = self.ContainsKey(item.Key) ? remapper(item.Key, self[item.Key], item.Value) : item.Value;
            }
        }

        /// <summary>
        /// Union dictionaries. When in conflicts, by default incoming values take priority.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="self"></param>
        /// <param name="other"></param>
        public static IDictionary<T, TV> Merge<T, TV>(this IDictionary<T, TV> self, IDictionary<T, TV>? other, Remapper<T, TV>? remapper = null) where T : notnull
        {
            if (remapper == null)
                remapper = TakeRight;

            var rt = self.ToDictionary(entry => entry.Key, entry => entry.Value);

            if (other == null)
                return new Dictionary<T, TV>(rt);

            foreach (KeyValuePair<T, TV> item in other)
            {
                rt[item.Key] = self.ContainsKey(item.Key) ? remapper(item.Key, self[item.Key], item.Value) : item.Value;
            }

            return rt;
        }

        public static TV TakeRight<T, TV>(T key, TV left, TV right)
        {
            return right;
        }

        public static TV TakeLeft<T, TV>(T key, TV left, TV right)
        {
            return left;
        }

        /// <summary>
        /// Drop all keys with null value.
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IDictionary<TK, TV> NotNull<TK, TV>(this IDictionary<TK, TV?> self) where TK : notnull
        {
            return self.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value!);
        }

        public static ILookup<TK, TV> ToGroupedLookup<SK, SV, TK, TV>(
            this IEnumerable<KeyValuePair<SK, IEnumerable<SV>>> self,
            Func<KeyValuePair<SK, SV>, TK> keyProjector,
            Func<KeyValuePair<SK, SV>, TV> valueProjector
        ) where SK : notnull where TK : notnull
        {
            return self.SelectMany(x => x.Value.Select(y => new KeyValuePair<SK, SV>(x.Key, y)))
                       .ToLookup(x => keyProjector(x), x => valueProjector(x));
        }
    }
}