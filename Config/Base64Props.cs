using System.Globalization;
using Common.Lang.Extensions;
using Common.Serialization.Json;

namespace Common.Config;

/// <summary>
/// Implement a type-safe IProps with a string based value store.
/// </summary>
public class Base64Props : IProps
{
    private readonly IPropStore<string> _store;
    private readonly IJsonizer? _serializer;

    public Base64Props(IPropStore<string> store, IJsonizer? serializer = null)
    {
        _store = store;
        _serializer = serializer;
    }

    public T Get<T>(string key)
    {
        var value = _store.Get(key);
        if (value == null)
            throw new InvalidOperationException($"Missing {key}");
        return Parse(value, default(T), _serializer) ?? throw new InvalidOperationException($"Missing {key}");
    }

    public T Get<T>(string key, T defValue)
    {
        var value = _store.Get(key);
        if (value == null)
            return defValue;
        return Parse(value, defValue, _serializer) ?? defValue;
    }

    public T? Put<T>(string key, T? value)
    {
        var oldValue = _store.Get(key);

        _store.Put(key, Compose(value, _serializer));

        if (oldValue == null)
            return default;

        return Parse<T>(oldValue, default, _serializer);
    }

    public bool Has(string key)
    {
        return _store.Has(key);
    }

    public void Clear()
    {
        _store.Clear();
    }

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
}