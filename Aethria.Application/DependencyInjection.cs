using Aethria.Application.Behaviors;
using DispatchR.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Aethria.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddDispatchR(options =>
        {
            options.Assemblies.Add(typeof(DependencyInjection).Assembly);
            options.RegisterPipelines = true;
            options.RegisterNotifications = true;
            options.PipelineOrder =
            [
                typeof(ValidationResultPipelineBehavior<,>),
                typeof(ValidationStreamPipelineBehavior<,>)
            ];
        });

        return services;
    }
}
