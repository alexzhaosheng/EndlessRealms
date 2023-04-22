using ElectronNET.API;
using EndlessRealms.Core.Services;
using EndlessRealms.ElectronUi.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using EndlessRealms.Core.Utility;
using Serilog;
using EndlessRealms.ElectronUi.Services;
using EndlessRealms.ElectronUi.Pages;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .AddJsonFile("./appSettings.json", false, false)
    .AddJsonFile("./appSettings.dev.json", true, false)
    .Build();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


builder.WebHost.UseElectron(args);

builder.Services.AddElectron();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();



builder.Services.Configure<ChatGptApiSetting>("ChatGpt", config);

builder.Services.LoadServices(EndlessRealms.Core.TheAssembly.Assembly);
builder.Services.LoadServices(EndlessRealms.LocalEnv.TheAssembly.Assembly);
builder.Services.LoadServices(EndlessRealms.ElectronUi.TheAssembly.Assembly);

var logModel = new LogModel();
var logger = new LoggerConfiguration()
           .ReadFrom.Configuration(config)
           .WriteTo.UiLogSink(logModel)
           .CreateLogger();

builder.Services.AddSingleton(logModel);
builder.Services.AddSingleton<ILogService>(new UILogger(logger));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.StartAsync();

if (HybridSupport.IsElectronActive)
{
    var wnd = await Electron.WindowManager.CreateWindowAsync();
    wnd.Maximize();
}

app.WaitForShutdown();

