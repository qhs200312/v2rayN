using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace AmazTool;

internal class UpgradeApp
{
    public static void Upgrade(string fileName, int? processId = null)
    {
        Console.WriteLine($"{Resx.Resource.StartUnzipping}\n{fileName}");

        if (!File.Exists(fileName))
        {
            Console.WriteLine(Resx.Resource.UpgradeFileNotFound);
            return;
        }

        Console.WriteLine(Resx.Resource.TryTerminateProcess);
        try
        {
            if (processId.HasValue)
            {
                WaitForProcessExit(processId.Value);
            }

            var existing = Process.GetProcessesByName(Utils.V2rayN);
            foreach (var pp in existing)
            {
                var path = pp.MainModule?.FileName ?? "";
                if (path.StartsWith(Utils.GetPath(Utils.V2rayN)))
                {
                    pp?.Kill();
                    pp?.WaitForExit(1000);
                }
            }
        }
        catch (Exception ex)
        {
            // Access may be denied without admin right. The user may not be an administrator.
            Console.WriteLine(Resx.Resource.FailedTerminateProcess + ex.StackTrace);
        }

        Console.WriteLine(Resx.Resource.StartUnzipping);
        StringBuilder sb = new();
        try
        {
            var thisAppOldFile = $"{Utils.GetExePath()}.tmp";
            File.Delete(thisAppOldFile);
            var startupPath = Path.GetFullPath(Utils.StartupPath());
            var startupPrefix = startupPath.EndsWith(Path.DirectorySeparatorChar)
                ? startupPath
                : startupPath + Path.DirectorySeparatorChar;

            using var archive = ZipFile.OpenRead(fileName);
            foreach (var entry in archive.Entries)
            {
                try
                {
                    if (entry.Length == 0)
                    {
                        continue;
                    }

                    Console.WriteLine(entry.FullName);

                    var normalizedEntryName = entry.FullName.Replace('\\', '/');
                    var lst = normalizedEntryName.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (lst.Length == 1)
                    {
                        continue;
                    }

                    var fullName = Path.Combine(lst[1..]);
                    var entryOutputPath = Path.GetFullPath(Utils.GetPath(fullName));
                    if (!entryOutputPath.StartsWith(startupPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Skipped unsafe archive entry: {entry.FullName}");
                        continue;
                    }

                    if (string.Equals(Utils.GetExePath(), entryOutputPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(Utils.GetExePath(), thisAppOldFile);
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(entryOutputPath)!);
                    //In the bin folder, if the file already exists, it will be skipped
                    if (fullName.StartsWith("bin") && File.Exists(entryOutputPath))
                    {
                        continue;
                    }

                    TryExtractToFile(entry, entryOutputPath);

                    Console.WriteLine(entryOutputPath);
                }
                catch (Exception ex)
                {
                    sb.Append(ex.StackTrace);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(Resx.Resource.FailedUpgrade + ex.StackTrace);
            //return;
        }
        if (sb.Length > 0)
        {
            Console.WriteLine(Resx.Resource.FailedUpgrade + sb.ToString());
            //return;
        }

        Console.WriteLine(Resx.Resource.Restartv2rayN);
        Utils.Waiting(2);

        Utils.StartV2RayN();
    }

    private static void WaitForProcessExit(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            if (!process.WaitForExit(15_000))
            {
                process.Kill(true);
                process.WaitForExit(5_000);
            }
        }
        catch (ArgumentException)
        {
            // The application has already exited.
        }
    }

    private static bool TryExtractToFile(ZipArchiveEntry entry, string outputPath)
    {
        var retryCount = 5;
        var delayMs = 1000;

        for (var i = 1; i <= retryCount; i++)
        {
            try
            {
                entry.ExtractToFile(outputPath, true);
                return true;
            }
            catch
            {
                Thread.Sleep(delayMs * i);
            }
        }
        return false;
    }
}
