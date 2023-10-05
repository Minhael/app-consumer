using System.Text;
using System.Security.Cryptography;
using Ude;

namespace Common.Lang.Extensions;

public static class BytesExtensions
{
    /// <summary>
    /// Detect encoding and decode byte array into string
    /// </summary>
    /// <param name="self"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <param name="encoding"></param>
    /// <returns>Decoded string</returns>
    public static string Decode(this byte[] self, int offset, int length, Encoding? encoding = null)
    {
        var detector = new CharsetDetector();
        detector.Feed(self, offset, length);
        detector.DataEnd();
        var decoder = detector.Charset == null ? encoding ?? Encoding.UTF8 : Encoding.GetEncoding(detector.Charset);
        return decoder.GetString(self.Trim(decoder.GetPreamble()));
    }

    /// <summary>
    /// Encode string to byte array in UTF-8
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static byte[] Utf8(this string self)
    {
        return Encoding.UTF8.GetBytes(self);
    }

    /// <summary>
    /// Decode byte array to string in UTF-8
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static string Utf8(this byte[] self)
    {
        return Encoding.UTF8.GetString(self);
    }

    /// <summary>
    /// Encode Base64 string to byte array
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static byte[] Base64(this string self)
    {
        return Convert.FromBase64String(self);
    }

    /// <summary>
    /// Decode byte array to Base64 string
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static string Base64(this byte[] self)
    {
        return Convert.ToBase64String(self);
    }

    /// <summary>
    /// Encode long to byte array. Endianness depends on the machine.
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this long self)
    {
        return BitConverter.GetBytes(self);
    }

    /// <summary>
    /// Decode byte array to long . Endianness depends on the machine.
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static long ToLong(this byte[] self)
    {
        return BitConverter.ToInt64(self, 0);
    }

    public static string Base256(this byte[] self) => self.ToBase256String();
    public static byte[] Base256(this string self) => self.FromBase256String();

    public static byte[] Hash(this byte[] self, HashAlgorithm? algorithm = null)
    {
        if (algorithm == null)
            algorithm = SHA1.Create();
        return algorithm.ComputeHash(self);
    }

    public static byte[] Sha1(this byte[] self) => Hash(self, SHA1.Create());
    public static byte[] Sha256(this byte[] self) => Hash(self, SHA256.Create());
    public static byte[] Sha512(this byte[] self) => Hash(self, SHA512.Create());

    public static string Hex(this byte[] self)
    {
        return Convert.ToHexString(self);
    }

    public static byte[] Hex(this string self)
    {
        return Convert.FromHexString(self);
    }
}