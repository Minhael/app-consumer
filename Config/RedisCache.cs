using System.Globalization;
using Common.Lang.Extensions;
using Common.Serialization.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Common.Config
{
    public class RedisCache : ICache
    {
        #region Private Members

        private readonly IDistributedCache _cache;
        private readonly IJsonizer? _serializer;

        #endregion Private Members

        #region Constructors

        public RedisCache(IDistributedCache cache, IJsonizer? serializer = null)
        {
            _cache = cache;
            _serializer = serializer;
        }

        #endregion Constructors

        #region Public Methods

        #region Generic Methods

        public async Task<T?> Read<T>(string key, CancellationToken token)
        {
            var cacheResponse = await _cache.GetStringAsync(key, token);
            return cacheResponse != null ? Parse<T>(cacheResponse, default(T), _serializer) : default(T);
        }

        public async Task<T> Read<T>(string key, T defValue, CancellationToken token)
        {
            var cacheResponse = await _cache.GetStringAsync(key, token);
            if (cacheResponse != null)
            {
                var value = Parse<T>(cacheResponse, defValue, _serializer);
                return value ?? defValue;
            }
            return defValue;
        }

        public async Task<T> Read<T>(string key, Func<Task<T>> task, CancellationToken token = default)
        {
            var cacheResponse = await _cache.GetStringAsync(key, token);
            if (cacheResponse != null)
            {
                var value = Parse<T>(cacheResponse, default(T), _serializer);
                if (value != null)
                    return value;
            }

            var defValue = await task();
            await Write(key, defValue, token);
            return defValue;
        }

        public async Task<T> Read<T>(string key, Func<Task<T>> task, TimeSpan effective, CancellationToken token = default)
        {
            var cacheResponse = await _cache.GetStringAsync(key, token);
            if (cacheResponse != null)
            {
                var value = Parse<T>(cacheResponse, default(T), _serializer);
                if (value != null)
                    return value;
            }

            var newValue = await task();
            await Write(key, newValue, effective, token);
            return newValue;
        }

        public Task Write<T>(string key, T? value, CancellationToken token)
        {
            if (value == null)
                return _cache.RemoveAsync(key, token);
            return _cache.SetStringAsync(key, Compose(value, _serializer), token);
        }

        public Task Write<T>(string key, T? value, TimeSpan effective, CancellationToken token = default)
        {
            if (value == null)
                return _cache.RemoveAsync(key, token);
            return _cache.SetStringAsync(key, Compose(value, _serializer), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = effective }, token);
        }

        async Task ICache.Delete(string key, CancellationToken token)
        {
            await _cache.RemoveAsync(key, token);
        }

        #endregion Generic Methods

        #region Other Methods

        public async Task<bool> Has(string key, CancellationToken token)
        {
            var cacheResponse = await _cache.GetStringAsync(key, token);
            return cacheResponse != null ? true : false;
        }
        #endregion Other Methods

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Convert string to type T with default value. For primitive types, string are casted directly. For object types, string are parsed as JSON.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="defValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static T? Parse<T>(string self, T? defValue, IJsonizer? serializer = null)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Object:
                    return self.ParseJsonAs<T>(serializer);
                case TypeCode.String:
                    return self.SafeCast<T>();
                case TypeCode.Boolean:
                    return Convert.ToBoolean(self).SafeCast(defValue);
                case TypeCode.Byte:
                    return Convert.ToByte(self).SafeCast(defValue);
                case TypeCode.Char:
                    return Convert.ToChar(self).SafeCast(defValue);
                case TypeCode.DateTime:
                    return Convert.ToDateTime(self).SafeCast(defValue);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(self).SafeCast(defValue);
                case TypeCode.Double:
                    return Convert.ToDouble(self).SafeCast(defValue);
                case TypeCode.Int16:
                    return Convert.ToInt16(self).SafeCast(defValue);
                case TypeCode.Int32:
                    return Convert.ToInt32(self).SafeCast(defValue);
                case TypeCode.Int64:
                    return Convert.ToInt64(self).SafeCast(defValue);
                case TypeCode.SByte:
                    return Convert.ToSByte(self).SafeCast(defValue);
                case TypeCode.Single:
                    return Convert.ToSingle(self).SafeCast(defValue);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(self).SafeCast(defValue);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(self).SafeCast(defValue);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(self).SafeCast(defValue);
                case TypeCode.DBNull:
                    return default;
                default:
                    throw new InvalidOperationException($"Unsupported type: {typeof(T)}");
            }
        }

        /// <summary>
        /// Convert value of type T to string. For primitive types, value are casted directly. For object types, values are composed in JSON.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static string? Compose<T>(T self, IJsonizer? serializer = null)
        {
            if (self == null)
                return null;
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Object:
                    return self.ComposeJson(serializer);
                case TypeCode.String:
                    return self.SafeCast<string>();
                case TypeCode.Boolean:
                    return Convert.ToString(self.SafeCast<bool>());
                case TypeCode.Byte:
                    return Convert.ToString(self.SafeCast<byte>());
                case TypeCode.Char:
                    return Convert.ToString(self.SafeCast<char>());
                case TypeCode.DateTime:
                    return Convert.ToString(self.SafeCast<DateTime>(), CultureInfo.InvariantCulture);
                case TypeCode.Decimal:
                    return Convert.ToString(self.SafeCast<decimal>(), CultureInfo.InvariantCulture);
                case TypeCode.Double:
                    return Convert.ToString(self.SafeCast<double>(), CultureInfo.InvariantCulture);
                case TypeCode.Int16:
                    return Convert.ToString(self.SafeCast<short>());
                case TypeCode.Int32:
                    return Convert.ToString(self.SafeCast<int>());
                case TypeCode.Int64:
                    return Convert.ToString(self.SafeCast<long>());
                case TypeCode.SByte:
                    return Convert.ToString(self.SafeCast<sbyte>());
                case TypeCode.Single:
                    return Convert.ToString(self.SafeCast<float>(), CultureInfo.InvariantCulture);
                case TypeCode.UInt16:
                    return Convert.ToString(self.SafeCast<ushort>());
                case TypeCode.UInt32:
                    return Convert.ToString(self.SafeCast<uint>());
                case TypeCode.UInt64:
                    return Convert.ToString(self.SafeCast<ulong>());
                case TypeCode.DBNull:
                    return null;
                default:
                    throw new InvalidOperationException($"Unsupported type: {typeof(T)}");
            }
        }

        #endregion Private Methods

    }

}
