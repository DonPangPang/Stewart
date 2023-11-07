using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Soda.Http.Extensions;
using Stewart.Client;
using Stewart.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMasaBlazor();
builder.Services.AddSodaHttp(opts =>
{
    opts.BaseUrl = builder.HostEnvironment.BaseAddress;
    opts.EnableCompress = true;
});
builder.Services.AddScoped<ProcessService>();

await builder.Build().RunAsync();