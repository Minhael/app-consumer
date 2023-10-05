using Common.Lang.IO;
using Common.Serialization.Json;
using Common.Telemetry;
using OpenTelemetry.Trace;

namespace Common.IO;

public class ResourceLoader
{
    private static readonly Tracer _tracer = Measure.CreateTracer<ResourceLoader>();

    private readonly string _rootDirectory;

    public ResourceLoader(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
    }

    public string LoadFile(string fileName)
    {
        var filePath = Path.Combine(_rootDirectory, fileName);
        var attr = new SpanAttributes();
        attr.Add("file.path", filePath);
        using var span = _tracer.StartActiveSpan($"LOAD file.{_rootDirectory}", SpanKind.Client, initialAttributes: attr);
        return FileX.ReadAllText(filePath);
    }

    public string? LoadFileOrNull(string fileName)
    {
        var filePath = Path.Combine(_rootDirectory, fileName);
        var attr = new SpanAttributes();
        attr.Add("file.path", filePath);
        using var span = _tracer.StartActiveSpan($"LOAD file.{_rootDirectory}", SpanKind.Client, initialAttributes: attr);
        return FileX.ReadAllTextOrNull(filePath);
    }

    public async Task<string> LoadFileAsync(string fileName, CancellationToken token = default)
    {
        var filePath = Path.Combine(_rootDirectory, fileName);
        var attr = new SpanAttributes();
        attr.Add("file.path", filePath);
        using var span = _tracer.StartActiveSpan($"LOAD file.{_rootDirectory}", SpanKind.Client, initialAttributes: attr);
        return await FileX.ReadAllTextAsync(filePath, cancellationToken: token);
    }

    public async Task<string?> LoadFileOrNullAsync(string fileName, CancellationToken token = default)
    {
        var filePath = Path.Combine(_rootDirectory, fileName);
        var attr = new SpanAttributes();
        attr.Add("file.path", filePath);
        using var span = _tracer.StartActiveSpan($"LOAD file.{_rootDirectory}", SpanKind.Client, initialAttributes: attr);
        return await FileX.ReadAllTextOrNullAsync(filePath, cancellationToken: token);
    }

    public async Task<T> LoadAs<T>(string fileName, int bufferSize = 4096, CancellationToken token = default) where T : class
    {
        var filePath = Path.Combine(_rootDirectory, fileName);
        var attr = new SpanAttributes();
        attr.Add("file.path", filePath);
        attr.Add("resource.type", typeof(T).FullName);
        using var span = _tracer.StartActiveSpan($"LOAD file.{_rootDirectory}", SpanKind.Client, initialAttributes: attr);
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: bufferSize, useAsync: true);
        return await fs.ParseJsonAs<T>(token: token);
    }

    public async Task<T?> LoadAsOrNull<T>(string fileName, int bufferSize = 4096, CancellationToken token = default) where T : class
    {
        var filePath = Path.Combine(_rootDirectory, fileName);
        if (!File.Exists(filePath))
            return null;

        var attr = new SpanAttributes();
        attr.Add("file.path", filePath);
        attr.Add("resource.type", typeof(T).FullName);
        using var span = _tracer.StartActiveSpan($"LOAD file.{_rootDirectory}", SpanKind.Client, initialAttributes: attr);
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: bufferSize, useAsync: true);
        return await fs.ParseJsonAs<T>(token: token);
    }

    public FileStream? LoadFileAsStreamOrNull(string fileName, int bufferSize = 4096)
    {
        var filePath = Path.Combine(_rootDirectory, fileName);
        if (!File.Exists(filePath))
            return null;

        var attr = new SpanAttributes();
        attr.Add("file.path", filePath);
        using var span = _tracer.StartActiveSpan($"LOAD file.{_rootDirectory}", SpanKind.Client, initialAttributes: attr);
        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
    }
}