using System.Reflection;
using ConvenienceStore.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ConvenienceStore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Đăng ký MediatR và tự động scan toàn bộ Handler trong Application assembly
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            // Đăng ký Behaviors theo thứ tự hành trình của Request
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Đăng ký FluentValidation scan toàn bộ Validator
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
