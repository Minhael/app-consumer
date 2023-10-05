using System.Text;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

namespace Common.Telemetry.OpenTelemetry;

public static class DictionaryPropagator
{
    private static readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;

    public static void Prepare(Dictionary<string, object> carrier)
    {
        _propagator.Inject(new PropagationContext(Tracer.CurrentSpan.Context, Baggage.Current), carrier, InjectTraceContextIntoBasicProperties);
    }

    public static PropagationContext Resume(Dictionary<string, object> carrier)
    {
        var parentContext = _propagator.Extract(default, carrier, ExtractTraceContextFromDictionary);
        Baggage.Current = parentContext.Baggage;
        return parentContext;
    }

    private static void InjectTraceContextIntoBasicProperties(Dictionary<string, object> carrier, string key, string value)
    {
        carrier[key] = value;
    }

    private static IEnumerable<string> ExtractTraceContextFromDictionary(Dictionary<string, object> carrier, string key)
    {
        try
        {
            if (carrier.TryGetValue(key, out var value))
            {
                switch (value)
                {
                    case string str:
                        return new[] { str };
                    case byte[] bytes:
                        return new[] { Encoding.UTF8.GetString(bytes) };
                }
            }
        }
        catch (Exception)
        {
        }

        return Enumerable.Empty<string>();
    }
}