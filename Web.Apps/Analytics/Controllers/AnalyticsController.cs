using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Logging;
using System.Text;
using Web.Apps.Analytics.Models;
using Web.AssetBundles.Models;
using Web.AssetBundles.Services;
using Web.Launcher.Models;

// ReSharper disable RouteTemplates.ControllerRouteParameterIsNotPassedToMethods
// ReSharper disable RouteTemplates.MethodMissingRouteParameters

namespace Web.Apps.Analytics.Controllers;

[Route("/Analytics/{AnalyticsKey}")]
public class AnalyticsController : Controller
{
    private readonly AssetBundleStaticConfig _aConfig;
    private readonly StartConfig _config;
    private readonly ILogger<AnalyticsController> _logger;
    private readonly ReplaceCaches _replaceCaches;
    private readonly FileLogger _fileLogger;

    public AnalyticsController(ILogger<AnalyticsController> logger, StartConfig config, ReplaceCaches replaceCaches,
        AssetBundleStaticConfig aConfig, FileLogger fileLogger)
    {
        _logger = logger;
        _config = config;
        _replaceCaches = replaceCaches;
        _aConfig = aConfig;
        _fileLogger = fileLogger;
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

        switch (labels[0])
        {
            case "ErrorsType" when labels.Count < 2:
                _logger.LogWarning("Unknown analytics error");
                break;
            case "ErrorsType":
            {
                var errorInfo = labels[1].Split('-');
                var errorType = errorInfo[0];

                switch (errorType)
                {
                    case "Fatal":
                        var errorId = int.Parse(errorInfo[1]);
                        _logger.LogError("Client ran into fatal error: {ErrorId}", errorId);

                        if (errorId is 2306 or 2302 or 2305)
                        {
                            _logger.LogError("Error likely due to caching system. Replacing!");
                            _replaceCaches.ReplaceWebPlayerCache();
                        }

                        break;
                    default:
                        _logger.LogWarning("Unknown error type: {ErrorType}", errorType);
                        break;
                }

                break;
            }
            case "Omniture":
            {
                if (n == "applicationStart")
                    if (_replaceCaches.CurrentlyLoadedAssets.Count > 0 && _aConfig.UseCacheReplacementScheme)
                        _replaceCaches.ReplaceWebPlayerCache();
                break;
            }
        }
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

    public void SendLog(CommonProperties properties, string message)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{properties.Timestamp:g}");
        sb.Append(message);

        _fileLogger.WriteGenericLog<global::Analytics>("analytics", $"Session {properties.SessionId}",
            sb.ToString(), LoggerType.Debug);
    }
}
