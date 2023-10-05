using System.Text;

namespace Common.Lang.Extensions
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Pull every single byte from the stream into heap. THIS BLOCKS. USE WITH CAUTION or consider asynchronous reading.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(this Stream self)
        {
            switch (self)
            {
                case MemoryStream ss:
                    return ss.ToArray();
                default:
                    using (var output = new MemoryStream())
                    {
                        self.CopyTo(output);
                        return output.ToArray();
                    }
            }
        }

        /// <summary>
        /// Reset stream to beginning.
        /// </summary>
        /// <param name="self"></param>
        public static void Flip(this Stream self)
        {
            self.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Remove bytes that are read and move unread bytes to the beginning.
        /// </summary>
        /// <param name="self"></param>
        public static void Compact(this MemoryStream self)
        {
            var offset = (int)self.Position;
            var length = (int)self.Length - offset;

            Buffer.BlockCopy(self.GetBuffer(), offset, self.GetBuffer(), 0, length);
            self.Seek(length, SeekOrigin.Begin);
            self.SetLength(length);
        }

        /// <summary>
        /// Decode stream to string in UTF-8.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ReadString(this MemoryStream self) => ReadString(self, Encoding.UTF8);

        /// <summary>
        /// Decode stream to string.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadString(this MemoryStream self, Encoding encoding)
        {
            return new StreamReader(self, encoding).ReadToEnd();
        }
    }
}