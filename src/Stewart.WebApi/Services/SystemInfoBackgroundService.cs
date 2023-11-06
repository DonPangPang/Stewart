using Microsoft.AspNetCore.SignalR;
using Stewart.Shared;
using Stewart.WebApi.Common;
using Stewart.WebApi.Hubs;

namespace Stewart.WebApi.Services;

public class SystemInfoBackgroundService : BackgroundService
{
    private readonly SystemInfoFactory _systemInfoFactory;
    private readonly IHubContext<SystemInfoHub, INotifyHubClient> _hubContext;

    public SystemInfoBackgroundService(SystemInfoFactory systemInfoFactory
    , IHubContext<SystemInfoHub, INotifyHubClient> hubContext)
    {
        _systemInfoFactory = systemInfoFactory;
        _hubContext = hubContext;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var service = _systemInfoFactory.CreateSystemInfoService();

        Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var info = new ServerInfo()
                    {
                        CpuInfo = service.GetCpuInfo(),
                        MemoryInfo = service.GetMemoryInfo(),
                        NetWorkInfo = service.GetNetWorkInfo(),
                        PlatformInfo = service.GetPlatformInfo()
                    };

                    await _hubContext.Clients.All.ReceiveMessage("notify", info);
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
        }, stoppingToken);

        return Task.CompletedTask;
    }
}