using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Web.Apps.Analytics.Models;
using Web.Launcher.Models;

// ReSharper disable RouteTemplates.ControllerRouteParameterIsNotPassedToMethods
// ReSharper disable RouteTemplates.MethodMissingRouteParameters

namespace Web.Apps.Analytics.Controllers;

[Route("/Analytics/{AnalyticsKey}")]
public class AnalyticsController : Controller
{
    private readonly ILogger<AnalyticsController> _logger;
    private readonly StartConfig _config;

    public AnalyticsController(ILogger<AnalyticsController> logger, StartConfig config)
    {
        _logger = logger;
        _config = config;
    }

    // b = Birthday
    // g = Gender
    // f = Source
    [HttpGet("cpu")]
    public void TrackUserInfo([FromQuery] string f, [FromQuery] char g, [FromQuery] string b)
    {
        if (!CheckAnalytics())
            return;
        
        var properties = GetCommonProperties();
        SendLog(properties, $"User joined from source '{f}'. Year of birth: '{b}'. Gender: '{g}'.");
    }

    // u = Page Name
    [HttpGet("pgr")]
    public void TrackHeartbeat([FromQuery] string u)
    {
        if (!CheckAnalytics())
            return;

        var properties = GetCommonProperties();
        SendLog(properties, $"Page '{u}' received.");
    }

    // n = Page Name
    // v = Arbitrary
    // l = Level
    // data = Json Data
    // st 1/2/3 = Labels
    [HttpGet("evt")]
    public void TrackCustomEvent([FromQuery] string n, [FromQuery] int v, [FromQuery] ushort l,
        [FromQuery] string data, [FromQuery] string st1, [FromRoute] string st2, [FromQuery] string st3)
    {
        if (!CheckAnalytics())
            return;

        var properties = GetCommonProperties();

        var labels = new List<string>();

        var messages = new List<string> { "Custom Event" };

        if (!string.IsNullOrEmpty(n))
            messages.Add($"Page Name: {n}");
        
        if (v > 0)
            messages.Add($"Value: {v}");

        if (l > 0)
            messages.Add($"Level: {l}");

        if (!string.IsNullOrEmpty(st1))
            labels.Add(st1);
        if (!string.IsNullOrEmpty(st2))
            labels.Add(st2);
        if (!string.IsNullOrEmpty(st3))
            labels.Add(st3);

        if (labels.Count > 0)
            messages.Add($"Labels: {string.Join(", ", labels)}");

        if (!string.IsNullOrEmpty(data))
        {
            var bytes = Convert.FromBase64String(data);
            var jsonData = Encoding.UTF8.GetString(bytes);

            dynamic parsedJson = JsonConvert.DeserializeObject(jsonData);
            var json = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);

            messages.Add($"Json: {json}");
        }

        SendLog(properties, string.Join('\n', messages));
    }

    public bool CheckAnalytics()
    {
        var analyticsKey = Request.RouteValues["AnalyticsKey"] as string;

        if (_config.AnalyticsApiKey == analyticsKey)
            return true;

        _logger.LogError("Client {ClientIp} sent invalid API key: {ApiKey}", Request.Host, analyticsKey);
        return false;
    }

    public CommonProperties GetCommonProperties()
    {
        var sessionId = ulong.Parse(Request.Query["s"].First());
        var timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(Request.Query["ts"].First()));
        return new CommonProperties(sessionId, timestamp);
    }

    public void SendLog(CommonProperties properties, string message) =>
        _logger.LogDebug("[{Timestamp:g}] Session {Id}: {Message}",
            properties.Timestamp, properties.SessionId, message);
}
