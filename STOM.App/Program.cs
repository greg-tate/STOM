using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using STOM.App;
using STOM.App.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var apiBaseAddress = $"http://{new Uri(builder.HostEnvironment.BaseAddress).Host}:5022/api/";
    return new HttpClient { BaseAddress = new Uri(apiBaseAddress) };
});
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AdminApiService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CacheService>();
builder.Services.AddScoped<OfflineStorageService>();
builder.Services.AddScoped<OfflineCapabilityService>();
builder.Services.AddScoped<SyncService>();

var host = builder.Build();

await host.RunAsync();




