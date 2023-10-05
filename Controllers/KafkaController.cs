using Common.Lang.Extensions;
using Common.Lang.Threading;
using Common.Messenger;
using Common.Messenger.EventHub;
using Common.Messenger.Kafka;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Serilog;
using System.Diagnostics;
using System.Net;

namespace App.Consumer.Controllers
{
    [ApiController]
    public class KafkaController : ControllerBase
    {
        private static readonly Serilog.ILogger _logger = Log.ForContext<EventController>();

        [HttpGet]
        [Route("kafka/{group}")]
        public async Task Consume(string group, [FromQuery] string[] topics, CancellationToken token = default)
        {
            await using var messenger = KafkaMessenger.BuildSaslPlain(
                "cds-web-vms-test-eh-0.servicebus.windows.net:9093",
                "$Default",
                "$ConnectionString",
                "Endpoint=sb://cds-web-vms-test-eh-0.servicebus.windows.net/;SharedAccessKeyName=HorizonApi;SharedAccessKey=k0X4pbmWELnHg9/N/ClO4LDdwrmcse2SJFljY4c6YyQ=",
                topics,
                new Dictionary<string, string> { }
            );

            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Content-Type", "text/event-stream");
            await Response.Body.FlushAsync(token);

            var disposables = topics.Select(x => messenger.Consume(x).Subscribe(
                async (m, ct) =>
                {
                    await Response.WriteAsync($"data: [{x}] {DateTime.UtcNow}> {m.Payload}\n", token);
                    await Response.WriteAsync("\n", token);
                    await Response.Body.FlushAsync(token);
                }
            )).ToArray();

            await disposables.WaitUntilCancelled(token);
        }

        [HttpPost]
        [Route("kafka/{group}/{topic}")]
        public async Task<IActionResult> Produce(string group, string topic, [FromBody] string payload, CancellationToken token = default)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            await using var messenger = KafkaMessenger.BuildSaslPlain(
                "cds-web-vms-test-eh-0.servicebus.windows.net:9093",
                "$Default",
                "$ConnectionString",
                "Endpoint=sb://cds-web-vms-test-eh-0.servicebus.windows.net/;SharedAccessKeyName=HorizonApi;SharedAccessKey=k0X4pbmWELnHg9/N/ClO4LDdwrmcse2SJFljY4c6YyQ=",
                new[] { topic },
                new Dictionary<string, string> { }
            );

            var message = new Message
            {
                Payload = payload,
                Key = payload.Utf8().Base64(),
                Timestamp = SystemClock.Instance.GetCurrentInstant(),
            };

            await messenger.Produce(topic).Publish(message, token);

            return NoContent();
        }

        // [HttpPatch]
        // [Route("kafka/{group}/{topic}")]
        // public async Task Ping(string group, string topic, [FromQuery] int delay = 2000, CancellationToken token = default)
        // {
        //     await using var messenger = new HubMessenger(
        //         Environment.GetEnvironmentVariable("AzureStorageConnectionString") ?? "",
        //         "eventhub-topic-offsets",
        //         Environment.GetEnvironmentVariable("EventHubConnectionString") ?? "",
        //         new[] { topic },
        //         new Dictionary<string, string> { { "groupID", group } }
        //     );
        //     var producer = messenger.Produce(topic);

        //     Response.Headers.Add("Cache-Control", "no-cache");
        //     Response.Headers.Add("Content-Type", "text/event-stream");
        //     await Response.Body.FlushAsync(token);

        //     var watch = new Stopwatch();
        //     watch.Start();

        //     try
        //     {
        //         while (!token.IsCancellationRequested)
        //         {
        //             var payload = $"{DateTime.UtcNow}";
        //             var message = new Message
        //             {
        //                 Payload = payload,
        //                 Key = payload.Utf8().Base64(),
        //                 Timestamp = SystemClock.Instance.GetCurrentInstant(),
        //             };

        //             var init = watch.Elapsed;
        //             try
        //             {
        //                 _logger.Debug("Ping {topic} - {payload}", topic, payload);
        //                 await producer.Publish(message, token);
        //                 var elapsed = watch.Elapsed - init;

        //                 await Response.WriteAsync($"event: success\n", token);
        //                 await Response.WriteAsync($"data: Sent to {topic}: time={elapsed.Milliseconds}ms\n", token);
        //                 await Response.WriteAsync($"\n", token);
        //                 await Response.Body.FlushAsync(token);
        //             }
        //             catch (Exception e)
        //             {
        //                 var elapsed = watch.Elapsed - init;

        //                 await Response.WriteAsync($"event: {e.GetType().Name}\n", token);
        //                 await Response.WriteAsync($"data: Failed {topic}: time={elapsed.Milliseconds}ms\n", token);
        //                 await Response.WriteAsync($"data: {e.Message}", token);
        //                 await Response.WriteAsync($"\n", token);
        //                 await Response.Body.FlushAsync(token);
        //             }

        //             if (delay > 0)
        //             {
        //                 _logger.Debug("Pause for {delay}ms", delay);
        //                 await Task.Delay(delay, token);
        //             }
        //         }
        //     }
        //     catch (TaskCanceledException) { }
        //     catch (OperationCanceledException) { }
        //     catch (Exception e)
        //     {
        //         _logger.Error(e, "Failed to produce events");
        //     }
        //     finally
        //     {
        //         watch.Stop();
        //     }
        // }
    }
}
