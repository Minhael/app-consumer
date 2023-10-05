using Common.Lang.Extensions;
using Common.Serialization.Json;
using Confluent.Kafka;
using Nito.Disposables;
using NodaTime;
using Serilog;

namespace Common.Messenger.Kafka;

// https://docs.confluent.io/platform/current/clients/confluent-kafka-dotnet/_site/api/Confluent.Kafka.html
public sealed class KafkaMessenger : IMessenger
{
    public static KafkaMessenger BuildSaslPlain(string uri, string groupId, string username, string password, string[] topics, IDictionary<string, string>? properties = null)
    {
        var producerConfig = new ProducerConfig(properties)
        {
            BootstrapServers = uri,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = "$ConnectionString",
            SaslPassword = password,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None,
            SocketTimeoutMs = 60000,                //this corresponds to the Consumer config `request.timeout.ms`
            SocketConnectionSetupTimeoutMs = 60000,
            SslCipherSuites = "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA",
            ClientId = Guid.NewGuid().ToString(),
            Debug = "security,broker,protocol"
        };
        var consumerConfig = new ConsumerConfig(properties)
        {
            BootstrapServers = uri,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SocketTimeoutMs = 60000,                //this corresponds to the Consumer config `request.timeout.ms`
            SessionTimeoutMs = 30000,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = "$ConnectionString",
            SaslPassword = password,
            GroupId = groupId,
            BrokerVersionFallback = "1.0.0",
            EnableAutoCommit = false,
        };

        return new KafkaMessenger(
            new ProducerBuilder<string, string>(producerConfig).SetLogHandler(LogHandler).SetErrorHandler(ErrorHandler).Build(),
            consumerConfig,
            topics
        );
    }

    private static readonly Serilog.ILogger _log = Log.ForContext<KafkaMessenger>();

    private IProducer<string, string> Producer { get; init; }
    private ConsumerConfig Consumer { get; init; }
    public string[] Topics { get; init; }

    public KafkaMessenger(IProducer<string, string> producer, ConsumerConfig consumer, string[] topics)
    {
        Producer = producer;
        Consumer = consumer;
        Topics = topics;
    }

    public IProducer Produce(string topic, IDictionary<string, string>? properties = null) => new KafkaProducer(topic, Producer);

    public IConsumer Consume(string topic, IDictionary<string, string>? properties = null)
    {
        return new KafkaConsumer(
            topic,
            new ConsumerBuilder<string, string>(Consumer)
                .SetLogHandler(LogHandler)
                .SetErrorHandler(ErrorHandler)
        );
    }

    public void Dispose()
    {
        Producer.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return Producer.ToAsyncDisposable().DisposeAsync();
    }

    private static void LogHandler(IProducer<string, string> consumer, LogMessage log) => LogMessage(log);
    private static void ErrorHandler(IProducer<string, string> consumer, Error e) => LogError(e);
    private static void LogHandler(IConsumer<string, string> consumer, LogMessage log) => LogMessage(log);
    private static void ErrorHandler(IConsumer<string, string> consumer, Error e) => LogError(e);

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
            Timestamp = Instant.FromUnixTimeMilliseconds(self.Message.Timestamp.UnixTimestampMs),
            Properties = properties,
        };
    }
}