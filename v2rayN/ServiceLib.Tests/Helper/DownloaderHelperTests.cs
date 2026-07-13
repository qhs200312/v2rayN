using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using ServiceLib.Helper;
using Xunit;

namespace ServiceLib.Tests.Helper;

public class DownloaderHelperTests
{
    [Fact]
    public async Task DownloadStringAsync_WhenServerReturnsTooManyRequests_ShouldReturnNull()
    {
        var port = GetFreePort();
        var prefix = $"http://127.0.0.1:{port}/";
        using var listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        listener.Start();

        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var server = ServeTooManyRequests(listener, cancellation.Token);

        var result = await new DownloaderHelper().DownloadStringAsync(null, prefix, "v2rayN-test", 5);

        result.Should().BeNull();
        await cancellation.CancelAsync();
        listener.Stop();
        await server;
    }

    private static async Task ServeTooManyRequests(HttpListener listener, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync().WaitAsync(cancellationToken);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Close();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
