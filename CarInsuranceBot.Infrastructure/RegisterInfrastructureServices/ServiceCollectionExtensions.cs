namespace CarInsuranceBot.Infrastructure.RegisterInfrastructureServices;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // 1. Options binding
        services.Configure<MindeeVehiclePassportOptions>(config.GetSection(MindeeVehiclePassportOptions.Section));
        services.Configure<MindeeDriverRegOptions>(config.GetSection(MindeeDriverRegOptions.Section));
        services.Configure<TelegramOptions>(config.GetSection(TelegramOptions.Section));
        services.Configure<MindeeOptions>(config.GetSection(MindeeOptions.Section));
        services.Configure<GeminiOptions>(config.GetSection(GeminiOptions.Section));
        services.Configure<AdminOptions>(config.GetSection(AdminOptions.Section));
        services.Configure<AzureBlobContainerOptions>(config.GetSection(AzureBlobContainerOptions.Section));
        services.Configure<AzureStorageOptions>(config.GetSection(AzureStorageOptions.Section));

        // 2. EF Core
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(
                config.GetConnectionString("Default"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()
            ));


        // 3. External services
        services.AddHttpClient("openai");
        services.AddScoped<IGeminiService, GeminiService>();

        services.AddSingleton (sp =>
        {
            string conn = sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value.ConnectionString;
            return new BlobServiceClient(conn);
        });

        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var token = sp.GetRequiredService<IOptions<TelegramOptions>>().Value.BotToken;
            return new TelegramBotClient(token);
        });

        services.AddScoped<IFileStore, BlobFileStore>(); // for user docs
        services.AddScoped<IPolicyFileStore, PolicyBlobFileStore>(); // for policies
        services.AddScoped<IGeminiService, GeminiService>();
        services.AddScoped<IMindeeService, MindeeService>();
        services.AddSingleton<IOcrSimulationSwitch, OcrSimulationSwitch>();
        services.AddScoped<IAuditService, AuditService>();

        // 4. Persistence
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();

        return services;
    }
}