using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using MediatR;
using CarInsuranceBot.Application.Common.Interfaces;

namespace CarInsuranceBot.Application.RegisterApplicationServices
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            services.AddMediatR(assemblies);                     // MediatR ≤ 11
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly()); // FV ≤ 10

            return services;
        }
    }

}