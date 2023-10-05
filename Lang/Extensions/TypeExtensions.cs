using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Common.Serialization.Json;
using Mapster;
using MapsterMapper;

namespace Common.Lang.Extensions;

[DebuggerStepThrough]
public static class TypeExtensions
{
    /// <summary>
    /// Try cast object to type T. Return null if cast failed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static T? SafeCast<T>(this object? self) => self.SafeCast<T>(default);

    /// <summary>
    /// Try cast object to type T. Return default value if cast failed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static T? SafeCast<T>(this object? self, T? defValue)
    {
        return self is T t ? t : defValue;
    }

    /// <summary>
    /// Unsafe cast. By pass all runtime type check for performance. USE WITH CAUTIONS.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static T UnsafeCast<T>(this object self) where T : class
    {
        return Unsafe.As<T>(self);
    }
#if NET6_0_OR_GREATER
    /// <summary>
    /// Mark type as non-nullable. Functional method for operator !.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T NotNull<T>(this T? self, string? message = null, [CallerArgumentExpression("self")] string? paramName = default) where T : notnull
    {
        //  https://stackoverflow.com/questions/70430422/how-to-implicitly-convert-nullable-type-to-non-nullable-type
        if (self == null)
            throw new ArgumentNullException(paramName, message);
        return self;
    }

    public static T NotNull<T>(this T? self, string? message = null, [CallerArgumentExpression("self")] string? paramName = default) where T : unmanaged
    {
        //  https://stackoverflow.com/questions/70430422/how-to-implicitly-convert-nullable-type-to-non-nullable-type
        if (self == null)
            throw new ArgumentNullException(paramName, message);
        return (T)self;
    }
#else
    public static T NotNull<T>(this T? self, string? message = null) where T : notnull
    {
        //  https://stackoverflow.com/questions/70430422/how-to-implicitly-convert-nullable-type-to-non-nullable-type
        if (self == null)
            throw new ArgumentNullException("self", message);
        return self;
    }

    public static T NotNull<T>(this T? self, string? message = null) where T : unmanaged
    {
        //  https://stackoverflow.com/questions/70430422/how-to-implicitly-convert-nullable-type-to-non-nullable-type
        if (self == null)
            throw new ArgumentNullException("self", message);
        return (T)self;
    }
#endif

    public static T? If<T>(this T? self, Func<T?, bool> validator) where T : struct
    {
        return validator(self) ? self : null;
    }

    /// <summary>
    /// Enumerate an item to list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static List<T> AsList<T>(this T self)
    {
        return new List<T>().Also(l => l.Add(self));
    }

    /// <summary>
    /// Enumerate an item to array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static T[] AsArray<T>(this T self)
    {
        return self.AsList().ToArray();
    }

    /// <summary>
    /// Enumerate an item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static IEnumerable<T> Enumerate<T>(this T self)
    {
        return self.AsArray();
    }

    /// <summary>
    /// Read string as UTF-8 bytes stream.
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static Stream AsStream(this string self) => AsStream(self, Encoding.UTF8);


    /// <summary>
    /// Read string as stream.
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static Stream AsStream(this string self, Encoding encoding)
    {
        return new MemoryStream(encoding.GetBytes(self));
    }

    private static readonly IMapper DEFAULT_MAPPER = new Mapper(TypeAdapterConfig.GlobalSettings);

    public static T ToType<T>(this object self, IMapper? mapper = null)
    {
        if (mapper == null)
            mapper = DEFAULT_MAPPER;
        return mapper.Map<T>(self);
    }

    /// <summary>
    /// Recreate object to type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <returns></returns>
    public static T ToType<S, T>(this S self, IMapper? mapper = null)
    {
        if (mapper == null)
            mapper = DEFAULT_MAPPER;
        return mapper.Map<S, T>(self);
    }

    /// <summary>
    /// Recreate reference object to type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="reference"></param>
    /// <returns></returns>
    public static T ToType<S, T>(this S self, T reference, IMapper? mapper = null) where T : class
    {
        if (mapper == null)
            mapper = DEFAULT_MAPPER;
        return mapper.Map(self, reference);
    }

    /// <summary>
    /// Recreate object to type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static T ToType<T>(this object self, Type type, IMapper? mapper = null)
    {
        if (mapper == null)
            mapper = DEFAULT_MAPPER;
        return (T)mapper.Map(self, self.GetType(), type);
    }

    /// <summary>
    /// Recreate object to type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="type"></param>
    /// <param name="reference"></param>
    /// <returns></returns>
    public static T ToType<T>(this object self, Type type, object reference, IMapper? mapper = null)
    {
        if (mapper == null)
            mapper = DEFAULT_MAPPER;
        return (T)mapper.Map(self, reference, self.GetType(), type);
    }

    /// <summary>
    /// Create object from dictionary of keys and values to type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="jsonizer"></param>
    /// <returns></returns>
    //  https://stackoverflow.com/questions/4943817/mapping-object-to-dictionary-and-vice-versa
    public static T ToObject<T>(this IDictionary<string, object> source, IJsonizer? mux = null)
        where T : class
    {
        return source.ComposeJson(mux).ParseJsonAs<T>(mux);
    }

    /// <summary>
    /// Create dictionary of keys and values from object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="jsonizer"></param>
    /// <returns></returns>
    [Obsolete("Use ToType<Dictionary<string, object>>()")]
    public static IDictionary<string, object> ToDictionary<T>(this T self, IJsonizer? mux = null) where T : class
    {
        return self.ComposeJson(mux).ParseJsonAs<Dictionary<string, object>>(mux);
    }

    /// <summary>
    /// Nullable version of Convert.ChangeType
    /// </summary>
    /// <param name="value"></param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    //  https://stackoverflow.com/questions/18015425/invalid-cast-from-system-int32-to-system-nullable1system-int32-mscorlib
    public static object? ChangeType(this object? value, Type conversionType)
    {
        if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            if (value == null)
                return null;
            conversionType = Nullable.GetUnderlyingType(conversionType)!;
        }

        return Convert.ChangeType(value, conversionType);
    }

    /// <summary>
    /// Get all assignable super class types of the current type.
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetAssignableTypes(this Type self)
    {
        if (self.IsGenericType)
        {
            var definition = self.GetGenericTypeDefinition();
            var partial = definition.GetInterfaces().Where(x => x.IsGenericType).Select(x => x.GetGenericTypeDefinition());
            var concrete = definition.GetInterfaces().Where(x => !x.IsGenericType);
            return self.GetGenericArguments()
                       .Select(x => GetAssignableTypes(x))
                       .CartesianProduct()
                       .SelectMany(x => partial.Append(definition).Select(y => y.MakeGenericType(x.ToArray())))
                       .Concat(concrete);
        }

        return self.GetInterfaces().Append(self);
    }

    public static string? GetVersion(this Type self)
    {
        return self.Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
    }
}