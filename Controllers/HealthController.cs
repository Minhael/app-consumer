using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;

namespace App.Consumer.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class HealthController : ControllerBase
{
    private static readonly Serilog.ILogger _logger = Log.ForContext<HealthController>();

    [HttpGet]
    [Route("health")]
    public IActionResult Ping()
    {
        return Ok();
    }

    [HttpGet]
    [Route("health/log")]
    public string LogLevel()
    {
        _logger.Verbose("+ trace +");
        _logger.Debug("+ debug +");
        _logger.Information("+ info  +");
        _logger.Warning("+ warn  +");
        _logger.Error("+ error +");
        _logger.Fatal("+ fatal +");

        return Log.IsEnabled(LogEventLevel.Verbose) ? "verbose" :
               Log.IsEnabled(LogEventLevel.Debug) ? "debug" :
               Log.IsEnabled(LogEventLevel.Information) ? "information" :
               Log.IsEnabled(LogEventLevel.Warning) ? "warning" :
               Log.IsEnabled(LogEventLevel.Error) ? "error" :
               Log.IsEnabled(LogEventLevel.Fatal) ? "fatal" : "none";
    }

    [HttpPost]
    [Route("health/gc")]
    public IActionResult Gc()
    {
        GC.Collect();
        return NoContent();
    }
}
