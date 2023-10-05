using Confluent.Kafka;

namespace Common.Lang.Extensions
{

    public static class ArrayExtensions
    {
        /// <summary>
        /// Get the index of the item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="obj"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns>index of the item</returns>
        public static long IndexOf<T>(this T[] arr, T obj, long offset = 0, long? length = null)
        {
            if (length == null)
                length = arr.Length - offset;

            for (long i = offset; i < length; i++)
            {
                if (obj?.Equals(arr[i]) == true)
                    return i;
            }
            return -1;
        }

        //  https://stackoverflow.com/questions/47815660/does-c-sharp-7-have-array-enumerable-destructuring/47816773#47816773
        /// <summary>
        /// Array deconstructor.
        /// 
        /// var list = new [] { 1, 2, 3, 4 };
        /// var (first, rest) = list;
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="first"></param>
        /// <param name="rest"></param>
        public static void Deconstruct<T>(this T[] list, out T first, out T[] rest)
        {
            first = list[0];
            rest = list.Skip(1).ToArray();
        }

        public static void Deconstruct<T>(this T[] list, out T first, out T second, out T[] rest)
        {
            first = list[0];
            second = list[1];
            rest = list.Skip(2).ToArray();
        }

        public static void Deconstruct<T>(this T[] list, out T first, out T second, out T third, out T[] rest)
        {
            first = list[0];
            second = list[1];
            third = list[2];
            rest = list.Skip(3).ToArray();
        }

        public static void Deconstruct<T>(this T[] list, out T first, out T second, out T third, out T fourth, out T[] rest)
        {
            first = list[0];
            second = list[1];
            third = list[2];
            fourth = list[3];
            rest = list.Skip(4).ToArray();
        }

        public static void Deconstruct<T>(this T[] list, out T first, out T second, out T third, out T fourth, out T fifth, out T[] rest)
        {
            first = list[0];
            second = list[1];
            third = list[2];
            fourth = list[3];
            fifth = list[4];
            rest = list.Skip(5).ToArray();
        }

        /// <summary>
        /// Return item at the position.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="position"></param>
        /// <returns>item at the position</returns>
        public static T? At<T>(this T[] self, int position)
        {
            if (position < 0 || position >= self.Length)
                return default;

            return self[position];
        }

        /// <summary>
        /// Trim leading bytes matching a pattern of sequence.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trimBytes"></param>
        /// <returns>Trimmed byte array</returns>
        public static byte[] Trim(this byte[] self, byte[] trimBytes)
        {
            var offset = 0;
            var length = self.Length;

            if (self.Take(trimBytes.Length).SequenceEqual(trimBytes))
            {
                offset = trimBytes.Length;
                length = length - trimBytes.Length;
            }

            if (self.Skip(self.Length - trimBytes.Length).SequenceEqual(trimBytes))
            {
                length = length - trimBytes.Length;
            }

            return self.Skip(offset).Take(length).ToArray();
        }

        public static T[] SetOrAppend<T>(this T[] self, int index, T obj)
        {
            var result = new T[Math.Max(self.Length, index + 1)];
            Array.Copy(self, result, self.Length);
            result[index] = obj;
            return result;
        }

        public static T[] Push<T>(this T[] self, T obj)
        {
            return obj.Enumerate().Concat(self).ToArray();
        }

        public static (T, T[]) Pop<T>(this T[] self)
        {
            var (head, rest) = self;
            return (head, rest);
        }
    }
}