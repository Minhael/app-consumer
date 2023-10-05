using System.Diagnostics;
using System.Security.Cryptography;
using Common.Lang.Threading;
using Common.Serialization.Binary;
using Common.Serialization.Json;

namespace Common.Lang.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Convert and return the converted type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="self"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static TR Let<T, TR>(this T self, Func<T, TR> block)
        {
            return block(self);
        }

        /// <summary>
        /// Perform action and return the incoming type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static T Also<T>(this T self, Action<T> block)
        {
            block(self);
            return self;
        }

        /// <summary>
        /// Perform action without type returns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="action"></param>
        [DebuggerHidden]
        public static void Do<T>(this T self, Action<T> action)
        {
            action(self);
        }

        /// <summary>
        /// Recreate a completely new object with no references are shadowed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="jsonizer"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this T self, IJsonizer? jsonizer = null) where T : notnull
        {
            return self.ComposeJson(self.GetType(), jsonizer).ParseJsonAs<T>(self.GetType(), jsonizer);
        }

        /// <summary>
        /// Select the first non null value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static T? Coalesc<T>(this T? self, params T?[] values)
        {
            return self.Coalesc(values, (obj) => obj != null);
        }

        /// <summary>
        /// Select the first non null value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static T? Coalesc<T>(this T? self, T?[] values, Func<T?, bool>? condition = null)
        {
            if (condition == null)
                condition = (obj) => obj != null;

            return self.Enumerate().Concat(values).FirstOrDefault(s => condition(s));
        }

        public static byte[] Hash<T>(this T self, IBinarizer? binarizer = null, HashAlgorithm? algorithm = null)
        {
            if (algorithm == null)
                algorithm = SHA1.Create();
            return self.Serialize(binarizer).ToArray().Hash(algorithm);
        }
    }
}