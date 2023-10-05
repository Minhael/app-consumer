using System.Reflection;
using Common.Lang.Extensions;

namespace Common.Data;

public interface INormalizer
{
    bool CanApply(Type instanceType, PropertyInfo propertyInfo, NullabilityInfo nullabilityInfo);
    bool CanApply(Type instanceType, FieldInfo fieldInfo, NullabilityInfo nullabilityInfo);
    object? Normalize(PropertyInfo propertyInfo, NullabilityInfo nullabilityInfo, object instance, object? value);
    object? Normalize(FieldInfo fieldInfo, NullabilityInfo nullabilityInfo, object instance, object? value);
}

public abstract class Normalizer<T> : INormalizer
{
    public virtual bool CanApply(Type instanceType, PropertyInfo propertyInfo, NullabilityInfo nullabilityInfo)
    {
        return propertyInfo.PropertyType == typeof(T);
    }
    public virtual bool CanApply(Type instanceType, FieldInfo fieldInfo, NullabilityInfo nullabilityInfo)
    {
        return fieldInfo.FieldType == typeof(T);
    }

    protected abstract T? Normalize(PropertyInfo propertyInfo, NullabilityInfo nullabilityInfo, object instance, T? value);
    protected abstract T? Normalize(FieldInfo fieldInfo, NullabilityInfo nullabilityInfo, object instance, T? value);

    public object? Normalize(PropertyInfo propertyInfo, NullabilityInfo nullabilityInfo, object instance, object? value)
    {
        return Normalize(propertyInfo, nullabilityInfo, instance, value.SafeCast<T>());
    }

    public object? Normalize(FieldInfo fieldInfo, NullabilityInfo nullabilityInfo, object instance, object? value)
    {
        return Normalize(fieldInfo, nullabilityInfo, instance, value.SafeCast<T>());
    }
}

public abstract class Normalizer<S, T> : Normalizer<T> where S : class
{
    public override bool CanApply(Type instanceType, PropertyInfo propertyInfo, NullabilityInfo nullabilityInfo)
    {
        return instanceType == typeof(S) && propertyInfo.PropertyType == typeof(T);
    }
    public override bool CanApply(Type instanceType, FieldInfo fieldInfo, NullabilityInfo nullabilityInfo)
    {
        return instanceType == typeof(S) && fieldInfo.FieldType == typeof(T);
    }

    protected abstract T? Normalize(PropertyInfo propertyInfo, NullabilityInfo nullabilityInfo, S instance, T? value);
    protected abstract T? Normalize(FieldInfo fieldInfo, NullabilityInfo nullabilityInfo, S instance, T? value);

    protected override T? Normalize(PropertyInfo propertyInfo, NullabilityInfo nullabilityInfo, object instance, T? value)
    {
        return Normalize(propertyInfo, nullabilityInfo, instance.UnsafeCast<S>(), value);
    }

    protected override T? Normalize(FieldInfo fieldInfo, NullabilityInfo nullabilityInfo, object instance, T? value)
    {
        return Normalize(fieldInfo, nullabilityInfo, instance.UnsafeCast<S>(), value);
    }
}