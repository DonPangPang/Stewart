using System.Net.NetworkInformation;

namespace Stewart.Shared;

public class GlobalVars
{
    public const string SystemInfoHub = "/system";
}

public enum OsPlatform
{
    Unknown,
    Windows,
    Unix,
    Docker
}

public class ServerInfo
{
    public VCpuInfo CpuInfo { get; set; } = new();
    public VMemoryInfo MemoryInfo { get; set; } = new();
    public VNetWorkInfo NetWorkInfo { get; set; } = new();
    public VPlatformInfo PlatformInfo { get; set; } = new();
    public VProcessInfo ProcessInfo { get; set; } = new();
}

public class VCpuInfo
{
    public OsPlatform Platform { get; set; } = OsPlatform.Unknown;
    public double CpuPercentage { get; set; }
}

public class VMemoryInfo
{
    /// <summary>
    /// 物理内存字节数
    /// </summary>
    public ulong TotalPhysicalMemory { get; set; }

    /// <summary>
    /// 可用的物理内存字节数
    /// </summary>
    public ulong AvailablePhysicalMemory { get; set; }

    /// <summary>
    /// 已用物理内存字节数
    /// </summary>
    public ulong UsedPhysicalMemory => TotalPhysicalMemory - AvailablePhysicalMemory;

    /// <summary>
    /// 已用物理内存百分比，0~100，100表示内存已用尽
    /// </summary>
    public double UsedPercentage { get; set; }

    /// <summary>
    /// 虚拟内存字节数
    /// </summary>
    public ulong TotalVirtualMemory { get; set; }

    /// <summary>
    /// 可用虚拟内存字节数
    /// </summary>
    public ulong AvailableVirtualMemory { get; set; }

    /// <summary>
    /// 已用虚拟内存字节数
    /// </summary>
    public ulong UsedVirtualMemory => TotalVirtualMemory - AvailableVirtualMemory;

    public double UsedVirtualPercentage { get; set; }
}

public class VNetWorkInfo
{
    public string? Name { get; set; }
    public string? Speed { get; set; }
    public string? Dns { get; set; }
    public string? Up { get; set; }
    public string? Down { get; set; }
    public NetworkInterfaceType NetworkType { get; set; }
    public string? Mac { get; set; }
    public string? Trademark { get; set; }
}

public class VPlatformInfo
{
    /// <summary>
    /// 框架平台
    /// </summary>
    public string FrameworkDescription { get; set; }

    /// <summary>
    /// 运行时版本信息
    /// </summary>
    public string FrameworkVersion { get; set; }

    /// <summary>
    /// 操作系统架构
    /// </summary>
    public string OSArchitecture { get; set; }

    /// <summary>
    /// 操作系统类型
    /// </summary>
    public string OSPlatformID { get; set; }

    /// <summary>
    /// 操作系统内核版本
    /// </summary>
    public string OSVersion { get; set; }

    /// <summary>
    /// 操作系统描述
    /// </summary>
    public string OSDescription { get; set; }

    /// <summary>
    /// 本进程的架构
    /// </summary>
    public string ProcessArchitecture { get; set; }

    /// <summary>
    /// 当前计算机上的处理器数
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// 计算机名称
    /// </summary>
    public string MachineName { get; set; }
}

public class VProcessInfo
{
    public List<ProcessInfo> ProcessInfos { get; set; } = new();
}

public class ProcessInfo
{
    public int Pid { get; set; }
    public string Name { get; set; } = string.Empty;
}