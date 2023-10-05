using Common.Serialization.Json;
using NodaTime;

namespace Common.Messenger;

public record Message
{
    public string Payload { get; init; } = "";
    public string? Key { get; init; } = null;
    public Instant Timestamp { get; init; }
    public Dictionary<string, object> Properties { get; init; } = new Dictionary<string, object>();

    public override string ToString()
    {
        return this.ComposeJson();
    }
}