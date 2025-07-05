using CarInsuranceBot.Application.Common.Interfaces;
using CarInsuranceBot.Infrastructure.Persistence;
using CarInsuranceBot.Infrastructure.FileStorage;
using Microsoft.Extensions.DependencyInjection;
using CarInsuranceBot.Infrastructure.Options;
using CarInsuranceBot.Infrastructure.OCR;
using Microsoft.Extensions.Configuration;
using CarInsuranceBot.Application.OCR;
using CarInsuranceBot.Application.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace CarInsuranceBot.Infrastructure.RegisterInfrastructureServices;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // 1. Options binding
        services.Configure<TelegramOptions>(config.GetSection(TelegramOptions.Section));
        services.Configure<OpenAIOptions>(config.GetSection(OpenAIOptions.Section));
        services.Configure<MindeeOptions>(config.GetSection(MindeeOptions.Section));
        services.Configure<MindeeVehiclePassportOptions>(config.GetSection(MindeeVehiclePassportOptions.Section));
        services.Configure<MindeeDriverRegOptions>(config.GetSection(MindeeDriverRegOptions.Section));

        // 2. EF Core
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("Default")));

        services.AddSingleton<IFileStore, DiskFileStore>();


        // 3. External services
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var token = sp.GetRequiredService<IOptions<TelegramOptions>>().Value.BotToken;
            return new TelegramBotClient(token);
        });

        services.AddScoped<IOpenAIService, OpenAIService>();
        services.AddScoped<IMindeeService, MindeeService>();

        // 4. Persistence
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}