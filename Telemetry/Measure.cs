using System.Diagnostics;
using Common.Lang.Extensions;
using NodaTime;
using OpenTelemetry.Trace;

namespace Common.Telemetry;

[DebuggerStepThrough]
public static class Measure
{
    //  https://www.meziantou.net/monitoring-a-dotnet-application-using-opentelemetry.htm
    //  https://opentelemetry.io/docs/instrumentation/net/shim/#record-exceptions-in-spans
    //  https://github.com/open-telemetry/opentelemetry-dotnet
    //  https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md
    //  https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/examples/MicroserviceExample
    //  https://opentelemetry.io/docs/reference/specification/overview/
    public static Tracer CreateTracer<T>(string? version = null)
    {
        var self = typeof(T);
        return TracerProvider.Default.GetTracer(self.FullName, self.GetVersion() ?? version ?? "0.0.0.0");
    }

    public static Tracer CreateTracer(string name, string? version = null)
    {
        return TracerProvider.Default.GetTracer(name, version ?? "0.0.0.0");
    }

    public static long Time(Action block)
    {
        var start = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
        block();
        var end = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
        return end - start;
    }
    public static (T, long) Time<T>(Func<T> block)
    {
        var start = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
        var rt = block();
        var end = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
        return (rt, end - start);
    }

    public static async Task<(T, long)> TimeAsync<T>(Func<Task<T>> block)
    {
        var start = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
        var rt = await block();
        var end = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
        return (rt, end - start);
    }

    public static async Task<long> TimeAsync(Func<Task> block)
    {
        var start = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
        await block();
        var end = SystemClock.Instance.GetCurrentInstant().ToUnixTimestampMs();
        return end - start;
    }
}