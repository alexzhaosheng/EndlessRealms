using EndlessRealms.Core.Services;
using EndlessRealms.WebUi.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using EndlessRealms.Core.Utility;
using Serilog;
using EndlessRealms.WebUi.Services;
using EndlessRealms.WebUi.Pages;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .AddJsonFile("./appSettings.dev.json", true, false)
    .Build();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

builder.Services.AddSingleton<EndlessRealms.Models.Settings>();


builder.Services.LoadServices(EndlessRealms.Core.TheAssembly.Assembly);
builder.Services.LoadServices(EndlessRealms.LocalEnv.TheAssembly.Assembly);
builder.Services.LoadServices(EndlessRealms.WebUi.TheAssembly.Assembly);

var logModel = new LogModel();
var logger = new LoggerConfiguration()
           .ReadFrom.Configuration(config)
           .WriteTo.UiLogSink(logModel)           
           .CreateLogger();

builder.Services.AddSingleton(logModel);
builder.Services.AddSingleton<ILogService>(new UILogger(logger));


var app = builder.Build();

await SetupSettingsValue(app);

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

//ShowHostMessage(app);
app.WaitForShutdown();




void ShowHostMessage(WebApplication app)
{
    IConfiguration configuration = app.Services.GetRequiredService<IConfiguration>();

    string hostUrl = configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000";

    Console.Clear();

    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("Access application from the url: ");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(hostUrl);

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Close this window to close the application.");

    Console.ResetColor();   
}

async Task SetupSettingsValue(WebApplication app)
{
    var settings = app.Services.GetService<EndlessRealms.Models.Settings>()!;
    
    var loadedSetting = await app.Services.GetService<IPersistedDataProvider>()!.LoadSettings();
    if(loadedSetting != null)
    {
        loadedSetting.CopyTo(settings);
    }
    if (string.IsNullOrWhiteSpace(settings.ChatGptApiKey))
    {
        var section = app.Configuration.GetSection("ChatGpt");
        settings.ChatGptApiKey = section?.GetValue<string>("ApiKey")!;
    }
}
