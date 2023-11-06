using System.Diagnostics;
using CZGL.SystemInfo.Linux;
using CZGL.SystemInfo;
using System.Net.NetworkInformation;
using Stewart.Shared;

namespace Stewart.WebApi.Common;

public static class EnvironmentSelector
{
    public static OsPlatform GetEnvironment()
    {
        if (IsDocker())
        {
            return OsPlatform.Docker;
        }
        else
        {
            return Environment.OSVersion.Platform switch
            {
                PlatformID.Unix or
                    PlatformID.MacOSX => OsPlatform.Unix,
                PlatformID.WinCE or
                    PlatformID.Win32NT or
                    PlatformID.Win32S or
                    PlatformID.Win32Windows => OsPlatform.Windows,
                _ => throw new PlatformNotSupportedException(""),
            };
        }
    }

    private static bool IsDocker()
    {
        return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    }
}

public class SystemInfoFactory
{
    public readonly Dictionary<OsPlatform, ISystemInfoService> Service = new();

    public SystemInfoFactory()
    {
    }

    public ISystemInfoService CreateSystemInfoService()
    {
        return EnvironmentSelector.GetEnvironment() switch
        {
            OsPlatform.Windows => TryGetOrCreate(OsPlatform.Windows),
            OsPlatform.Unix or
            OsPlatform.Docker => TryGetOrCreate(OsPlatform.Unix),
            _ => throw new PlatformNotSupportedException(""),
        };
    }

    private ISystemInfoService TryGetOrCreate(OsPlatform key)
    {
        if (!Service.TryGetValue(key, out var service))
        {
            service = key switch
            {
                OsPlatform.Windows => new WindowsSystemInfoService(),
                OsPlatform.Docker or
                OsPlatform.Unix => new UnixSystemInfoService(),
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }

        return service;
    }
}

public interface ISystemInfoService
{
    public VCpuInfo GetCpuInfo();

    public VMemoryInfo GetMemoryInfo();

    public VNetWorkInfo GetNetWorkInfo();

    public VPlatformInfo GetPlatformInfo();

    public VProcessInfo GetProcessInfo();

    public void KillProcessByName(string name);

    public void KillProcessById(int id);
}

public class WindowsSystemInfoService : ISystemInfoService
{
    private static readonly NetworkInfo? Network = NetworkInfo.TryGetRealNetworkInfo();

    private static CPUTime _v1 { get; set; } = CPUHelper.GetCPUTime();
    private static Rate? _r1 { get; set; } = Network?.GetIpv4Speed();

    public VCpuInfo GetCpuInfo()
    {
        var v2 = CPUHelper.GetCPUTime();
        var per = CPUHelper.CalculateCPULoad(_v1, v2);
        _v1 = v2;
        return new VCpuInfo()
        {
            Platform = EnvironmentSelector.GetEnvironment(),
            CpuPercentage = Math.Round(per * 100, 2),
        };
    }

    public VMemoryInfo GetMemoryInfo()
    {
        var info = MemoryHelper.GetMemoryValue();
        return new VMemoryInfo()
        {
            TotalPhysicalMemory = info.TotalPhysicalMemory,
            AvailablePhysicalMemory = info.AvailablePhysicalMemory,
            UsedPercentage = info.UsedPercentage,
            TotalVirtualMemory = info.TotalVirtualMemory,
            AvailableVirtualMemory = info.AvailableVirtualMemory
        };
    }

    public VNetWorkInfo GetNetWorkInfo()
    {
        if (Network is null) return new VNetWorkInfo();

        var r2 = Network.GetIpv4Speed();

        var speed = NetworkInfo.GetSpeed(_r1 ?? new Rate(), r2);
        _r1 = r2;
        return new VNetWorkInfo()
        {
            Name = Network.Name,
            Speed = $"{(double)Network.Speed / 1000 / 1000} Mbps",
            Dns = string.Join(',', Network.DnsAddresses.Select(x => x.ToString()).ToArray()),
            Up = $"{speed.Sent.Size} {speed.Sent.SizeType}/s",
            Down = $"{speed.Received.Size} {speed.Received.SizeType}/s",
            NetworkType = Network.NetworkType,
            Mac = Network.Mac,
            Trademark = Network.Trademark,
        };
    }

    public VPlatformInfo GetPlatformInfo()
    {
        return new VPlatformInfo()
        {
            FrameworkDescription = SystemPlatformInfo.FrameworkDescription,
            FrameworkVersion = SystemPlatformInfo.FrameworkVersion,
            OSVersion = SystemPlatformInfo.OSVersion,
            OSDescription = SystemPlatformInfo.OSDescription,
            ProcessArchitecture = SystemPlatformInfo.OSArchitecture,
            ProcessorCount = SystemPlatformInfo.ProcessorCount,
            MachineName = SystemPlatformInfo.MachineName,
            OSArchitecture = SystemPlatformInfo.OSArchitecture,
            OSPlatformID = SystemPlatformInfo.OSPlatformID
        };
    }

    public VProcessInfo GetProcessInfo()
    {
        var processes = Process.GetProcesses();

        return new VProcessInfo()
        {
            ProcessInfos = processes.Select(x => new ProcessInfo() { Pid = x.Id, Name = x.ProcessName }).ToList()
        };
    }

    public void KillProcessByName(string name)
    {
        var process = Process.GetProcessesByName(name);
        foreach (var item in process)
        {
            item.Kill();
        }
    }

    public void KillProcessById(int id)
    {
        var process = Process.GetProcessById(id);
        process.Kill();
    }
}

public class UnixSystemInfoService : ISystemInfoService
{
    private static readonly DynamicInfo Info = new DynamicInfo();
    private static readonly NetworkInfo? Network = NetworkInfo.TryGetRealNetworkInfo();

    private static Rate? _r1 = Network?.GetIpv4Speed();

    public VCpuInfo GetCpuInfo()
    {
        try
        {
            var cpu = Info.GetCpuState();

            return new VCpuInfo()
            {
                Platform = EnvironmentSelector.GetEnvironment(),
                CpuPercentage = cpu.UserSpace
            };
        }
        catch { return new VCpuInfo(); }
    }

    public VMemoryInfo GetMemoryInfo()
    {
        try
        {
            var mem = Info.GetMem();
            var swap = Info.GetSwap();
            return new VMemoryInfo()
            {
                AvailablePhysicalMemory = (ulong)mem.CanUsed,
                TotalPhysicalMemory = (ulong)mem.Total,
                AvailableVirtualMemory = (ulong)swap.Used,
                TotalVirtualMemory = (ulong)swap.Total,
                UsedPercentage = Math.Round((double)mem.Used / mem.Total, 2),
                UsedVirtualPercentage = Math.Round((double)swap.Used / swap.Total, 2),
            };
        }
        catch
        {
            return new VMemoryInfo();
        }
    }

    public VNetWorkInfo GetNetWorkInfo()
    {
        try
        {
            if (Network is null) return new VNetWorkInfo();

            var r2 = Network.GetIpv4Speed();

            var speed = NetworkInfo.GetSpeed(_r1 ?? new Rate(), r2);
            _r1 = r2;
            return new VNetWorkInfo()
            {
                Name = Network.Name,
                Speed = $"{(double)Network.Speed / 1000 / 1000} Mbps",
                Dns = string.Join(',', Network.DnsAddresses.Select(x => x.ToString()).ToArray()),
                Up = $"{speed.Sent.Size} {speed.Sent.SizeType}/s",
                Down = $"{speed.Received.Size} {speed.Received.SizeType}/s",
                NetworkType = Network.NetworkType,
                Mac = Network.Mac,
                Trademark = Network.Trademark,
            };
        }
        catch { return new VNetWorkInfo(); }
    }

    public VPlatformInfo GetPlatformInfo()
    {
        try
        {
            return new VPlatformInfo()
            {
                FrameworkDescription = SystemPlatformInfo.FrameworkDescription,
                FrameworkVersion = SystemPlatformInfo.FrameworkVersion,
                OSVersion = SystemPlatformInfo.OSVersion,
                OSDescription = SystemPlatformInfo.OSDescription,
                ProcessArchitecture = SystemPlatformInfo.OSArchitecture,
                ProcessorCount = SystemPlatformInfo.ProcessorCount,
                MachineName = SystemPlatformInfo.MachineName,
                OSArchitecture = SystemPlatformInfo.OSArchitecture,
                OSPlatformID = SystemPlatformInfo.OSPlatformID
            };
        }
        catch { return new VPlatformInfo(); }
    }

    public VProcessInfo GetProcessInfo()
    {
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = "-c \"ps aux | awk '{print $11}'\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        List<ProcessInfo> infos = new();

        foreach (string line in output.Split('\n'))
        {
            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                Int32.TryParse(parts[1], out var processId);
                string processName = parts[10];
                infos.Add(new ProcessInfo()
                {
                    Pid = processId,
                    Name = processName
                });
            }
        }

        return new VProcessInfo()
        {
            ProcessInfos = infos
        };
    }

    public void KillProcessByName(string name)
    {
        var process = Process.GetProcessesByName(name);
        foreach (var item in process)
        {
            item.Kill();
        }
    }

    public void KillProcessById(int id)
    {
        var process = Process.GetProcessById(id);
        process.Kill();
    }
}