using Common.Lang.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace Common.Misc;

//  https://stackoverflow.com/questions/50651694/serilog-how-to-make-custom-console-output-format
public class SerilogClassEnricher : ILogEventEnricher
{
    private readonly int _maxLength;

    public SerilogClassEnricher(int maxLength = 36)
    {
        _maxLength = maxLength;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string[] typeName = logEvent.Properties.GetOrDefault("SourceContext")
                ?.ToString()
                ?.Skip(1)
                .ToList()
                .Let(it => it.Take(it.Count - 1))
                .Let(string.Concat)
                .Split('.')
                ?? Array.Empty<string>();
        var clazzName = typeName.LastOrDefault() ?? "";
        var components = typeName.Take(typeName.Length - 1).ToArray();
        var sum = components.Reverse().Select(it => it.Length).Scan(0, (acc, length) => acc + length + 1).Skip(1);
        var offset = components.Length - sum.TakeWhile((it, index) => it + clazzName.Length + (components.Length - index) * 2 < _maxLength).Count();
        var prefix = components.Take(offset).Select(it => it.First().ToString());
        var suffix = typeName.Skip(offset);

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("SourceContext", $"{string.Join(".", prefix.Concat(suffix))}"));
    }
}