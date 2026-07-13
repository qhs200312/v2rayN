using Microsoft.Win32;

namespace ServiceLib.Common;

[SupportedOSPlatform("windows")]
internal static class WindowsUtils
{
    private static readonly string _tag = "WindowsUtils";
    private static readonly HashSet<string> _tunInterfaceNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "singbox_tun",
        "xray_tun",
    };

    public static string? RegReadValue(string path, string name, string def)
    {
        RegistryKey? regKey = null;
        try
        {
            regKey = Registry.CurrentUser.OpenSubKey(path, false);
            var value = regKey?.GetValue(name) as string;
            return value.IsNullOrEmpty() ? def : value;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
        finally
        {
            regKey?.Close();
        }
        return def;
    }

    public static void RegWriteValue(string path, string name, object value)
    {
        RegistryKey? regKey = null;
        try
        {
            regKey = Registry.CurrentUser.CreateSubKey(path);
            if (value.ToString().IsNullOrEmpty())
            {
                regKey?.DeleteValue(name, false);
            }
            else
            {
                regKey?.SetValue(name, value);
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
        finally
        {
            regKey?.Close();
        }
    }

    public static async Task RemoveTunDevice()
    {
        var tunNameList = new List<string> { "wintunsingbox_tun", "xray_tun" };
        foreach (var tunName in tunNameList)
        {
            try
            {
                var sum = MD5.HashData(Encoding.UTF8.GetBytes(tunName));
                var guid = new Guid(sum);
                var pnpUtilPath = @"C:\Windows\System32\pnputil.exe";
                var arg = $$""" /remove-device  "SWD\Wintun\{{{guid}}}" """;

                // Try to remove the device
                _ = await Utils.GetCliWrapOutput(pnpUtilPath, arg);
            }
            catch (Exception ex)
            {
                Logging.SaveLog(_tag, ex);
            }
        }

        var timeout = TimeSpan.FromSeconds(5);
        var startedAt = Stopwatch.GetTimestamp();
        while (TunInterfaceExists() && Stopwatch.GetElapsedTime(startedAt) < timeout)
        {
            await Task.Delay(100);
        }

        if (TunInterfaceExists())
        {
            Logging.SaveLog("Timed out waiting for the previous TUN interface to be removed.");
        }
    }

    private static bool TunInterfaceExists()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Any(networkInterface => _tunInterfaceNames.Any(tunName =>
                    networkInterface.Name.Equals(tunName, StringComparison.OrdinalIgnoreCase)
                    || networkInterface.Name.StartsWith($"{tunName} ", StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
            return false;
        }
    }
}
