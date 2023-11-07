using Microsoft.AspNetCore.Mvc;
using Stewart.WebApi.Common;

namespace Stewart.WebApi.Controllers;

public class ProcessController : ApiControllerBase
{
    private readonly SystemInfoFactory _systemInfoFactory;

    public ProcessController(SystemInfoFactory systemInfoFactory)
    {
        _systemInfoFactory = systemInfoFactory;
    }

    [HttpGet("kill/{pid}")]
    public IActionResult KillProcess(int pid)
    {
        var service = _systemInfoFactory.CreateSystemInfoService();
        service.KillProcessById(pid);

        return Ok();
    }
}