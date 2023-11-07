using Soda.Http.Core;

namespace Stewart.Client.Services;

public class ProcessService
{
    private readonly HttpClient _http;

    public ProcessService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> KillProcessAsync(int pid)
    {
        var res = await _http.GetAsync($"api/Process/Kill/{pid}");

        return res.IsSuccessStatusCode;
    }
}