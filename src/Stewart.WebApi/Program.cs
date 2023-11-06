using Microsoft.AspNetCore.ResponseCompression;
using Stewart.Shared;
using Stewart.WebApi.Common;
using Stewart.WebApi.Hubs;
using Stewart.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR().AddMessagePackProtocol();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddSingleton<SystemInfoFactory>();
builder.Services.AddHostedService<SystemInfoBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true
    //ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
    //        {
    //                { ".apk", "application/vnd.android.package-archive" }
    //        })
});
app.UseResponseCompression();
//app.UseHttpsRedirection();
app.MapHub<SystemInfoHub>(GlobalVars.SystemInfoHub);
app.UseAuthorization();

app.MapControllers();

var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles();

app.Run();

app.Run();