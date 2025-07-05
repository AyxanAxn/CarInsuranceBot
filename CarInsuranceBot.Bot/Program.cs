
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        QuestPDF.Settings.License = LicenseType.Community;

        builder.Configuration
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                            optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();


        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)    // reads Serilog: section
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.Services.AddLogging(lb => lb.ClearProviders().AddSerilog());
        // DI – application & infrastructure layers
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        builder.Services.AddHostedService<TelegramBotWorker>(); // rename if your worker is called Worker

        var host = builder.Build();
        
        // Test log to verify logging is working
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Bot starting up - logging test successful!");
        
        host.Run();
    }
}