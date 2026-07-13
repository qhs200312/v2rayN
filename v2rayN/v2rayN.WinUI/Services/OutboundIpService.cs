using System.Net;
using ServiceLib;
using ServiceLib.Enums;
using ServiceLib.Handler;
using ServiceLib.Manager;

namespace v2rayN.WinUI.Services;

internal sealed class OutboundIpService
{
    public async Task<OutboundIpResult> DetectAsync()
    {
        var isCoreRunning = Enum.IsDefined(AppManager.Instance.RunningCoreType);
        if (isCoreRunning)
        {
            var port = AppManager.Instance.GetLocalPort(EInboundProtocol.socks);
            var proxy = new WebProxy($"socks5://127.0.0.1:{port}");
            var proxied = await DetectFromAvailableApis(proxy);
            if (proxied is { } proxiedValue && !string.IsNullOrWhiteSpace(proxiedValue.Ip))
            {
                return OutboundIpResult.From(proxiedValue, "代理出口");
            }
        }

        var direct = await DetectFromAvailableApis(null);
        if (direct is { } directValue && !string.IsNullOrWhiteSpace(directValue.Ip))
        {
            return OutboundIpResult.From(directValue, isCoreRunning ? "直连回退" : "直连出口");
        }

        throw new InvalidOperationException("无法获取出口 IP，请检查 IP API 地址或网络连接。");
    }

    private static async Task<ServiceLib.Models.Dto.IpInfoResult?> DetectFromAvailableApis(IWebProxy? proxy)
    {
        var configured = AppManager.Instance.Config.SpeedTestItem.IPAPIUrl;
        var urls = new[]
        {
            configured,
            "https://ipinfo.io/json",
            "https://ipwho.is/"
        }
        .Concat(Global.IPAPIUrls)
        .Append("https://api.ipify.org?format=json")
        .Where(url => !string.IsNullOrWhiteSpace(url))
        .Distinct(StringComparer.OrdinalIgnoreCase);

        ServiceLib.Models.Dto.IpInfoResult? ipOnlyResult = null;
        foreach (var url in urls)
        {
            var result = await ConnectionHandler.GetIPInfo(proxy, url);
            if (result is not { } value || string.IsNullOrWhiteSpace(value.Ip))
            {
                continue;
            }

            ipOnlyResult ??= value;
            if (!string.Equals(value.Country, "unknown", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(value.Region)
                || !string.IsNullOrWhiteSpace(value.City))
            {
                return value;
            }
        }
        return ipOnlyResult;
    }
}

internal sealed record OutboundIpResult(string Ip, string Route, string LocationText)
{
    public static OutboundIpResult From(ServiceLib.Models.Dto.IpInfoResult result, string route)
    {
        var locations = new[] { result.Country, result.Region, result.City }
            .Where(value => !string.IsNullOrWhiteSpace(value) && !string.Equals(value, "unknown", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        var location = string.Join(" · ", locations);
        return new OutboundIpResult(result.Ip!, route, string.IsNullOrWhiteSpace(location) ? $"{route} · 地区不可用" : $"{route} · {location}");
    }
}
