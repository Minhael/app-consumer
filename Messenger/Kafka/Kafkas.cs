
using Common.Serialization.Json;
using Confluent.Kafka;
using Serilog;

namespace Common.Messenger.Kafka;

public static class Kafkas
{
    private static readonly Serilog.ILogger _log = Log.ForContext<KafkaMessenger>();

    public static KafkaMessenger BuildSaslPlain(string uri, string groupId, string username, string password, string[] topics, IDictionary<string, string>? properties = null)
    {
        var producerConfig = new ProducerConfig(properties)
        {
            BootstrapServers = uri,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = username,
            SaslPassword = password,
            ClientId = Guid.NewGuid().ToString()
        };
        var consumerConfig = new ConsumerConfig(properties)
        {
            BootstrapServers = uri,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = username,
            SaslPassword = password,
            GroupId = groupId,
            EnableAutoCommit = false,
        };

        return new KafkaMessenger(
            new ProducerBuilder<string, string>(producerConfig).SetLogHandler(LogHandler).SetErrorHandler(ErrorHandler).Build(),
            consumerConfig,
            topics
        );
    }

    internal static void LogHandler(IProducer<string, string> consumer, LogMessage log) => LogMessage(log);
    internal static void ErrorHandler(IProducer<string, string> consumer, Error e) => LogError(e);
    internal static void LogHandler(IConsumer<string, string> consumer, LogMessage log) => LogMessage(log);
    internal static void ErrorHandler(IConsumer<string, string> consumer, Error e) => LogError(e);

    private static void LogMessage(LogMessage msg)
    {
        switch (msg.Level)
        {
            case SyslogLevel.Emergency:
                _log.Fatal("{Instance}> {Message}", msg.Name, msg.Message);
                break;
            case SyslogLevel.Alert:
                _log.Fatal("{Instance}> {Message}", msg.Name, msg.Message);
                break;
            case SyslogLevel.Critical:
                _log.Fatal("{Instance}> {Message}", msg.Name, msg.Message);
                break;
            case SyslogLevel.Error:
                _log.Error("{Instance}> {Message}", msg.Name, msg.Message);
                break;
            case SyslogLevel.Warning:
                _log.Warning("{Instance}> {Message}", msg.Name, msg.Message);
                break;
            case SyslogLevel.Notice:
                _log.Information("{Instance}> {Message}", msg.Name, msg.Message);
                break;
            case SyslogLevel.Info:
                _log.Information("{Instance}> {Message}", msg.Name, msg.Message);
                break;
            case SyslogLevel.Debug:
                _log.Debug("{Instance}> {Message}", msg.Name, msg.Message);
                break;
            default:
                _log.Verbose("{Instance}> {Message}", msg.Name, msg.Message);
                break;
        }
    }

    private static void LogError(Error e)
    {
        _log.Error("{Error} {Message}", e.Code, e.Reason);
    }
}

static class KafkaHelpers
{
    public static Message Convert(this ConsumeResult<string, string> self)
    {
        var properties = self.Message.Headers.ToDictionary(h => h.Key, h => h.GetValueBytes().ParseJsonAs<object>());
        return new Message
        {
            Payload = self.Message.Value,
            Key = self.Message.Key,
            Utc = self.Message.Timestamp.UnixTimestampMs,
            Properties = properties,
        };
    }
}