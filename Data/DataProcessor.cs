using System.Reflection;
using Common.Lang.Extensions;
using Common.Telemetry;
using OpenTelemetry.Trace;

namespace Common.Data;

public class DataProcessor
{
    private static readonly Tracer _tracer = Measure.CreateTracer<DataProcessor>();

    private readonly List<INormalizer> _normalizers;
    private readonly NullabilityInfoContext _context = new NullabilityInfoContext();

    public DataProcessor(IEnumerable<INormalizer> normalizers)
    {
        _normalizers = normalizers.ToList();
    }

    public void Normalize(object obj, Type instanceType)
    {
        using var span = _tracer.StartActiveSpan($"NORAMLIZE {obj.GetType().Name}", SpanKind.Internal);

        var properties = instanceType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.CanRead && x.CanWrite);
        var fields = instanceType.GetFields(BindingFlags.Instance | BindingFlags.Public);

        foreach (var property in properties)
        {
            var nullabilityInfo = _context.Create(property);
            var value = property.GetValue(obj);
            var valueType = property.PropertyType;
            var availableNormalizers = _normalizers.Where(x => x.CanApply(instanceType, property, nullabilityInfo));
            var normalized = availableNormalizers.Aggregate(value, (acc, x) => x.Normalize(property, nullabilityInfo, obj, acc));
            if (normalized != null && !valueType.IsValueType && valueType != typeof(string) && valueType != typeof(object))
                Normalize(normalized, valueType);
            property.SetValue(obj, normalized);
        }

        foreach (var field in fields)
        {
            var nullabilityInfo = _context.Create(field);
            var valueType = field.FieldType;
            var availableNormalizers = _normalizers.Where(x => x.CanApply(instanceType, field, nullabilityInfo));
            var normalized = availableNormalizers.Aggregate(field.GetValue(obj), (acc, x) => x.Normalize(field, nullabilityInfo, obj, acc));
            if (normalized != null && !valueType.IsValueType && valueType != typeof(string) && valueType != typeof(object))
                Normalize(normalized, valueType);
            field.SetValue(obj, normalized);
        }
    }
}

public static class DataProcessorExtensions
{
    public static void Normalize<T>(this DataProcessor self, T obj)
    {
        if (obj != null)
            self.Normalize(obj, typeof(T));
    }
}