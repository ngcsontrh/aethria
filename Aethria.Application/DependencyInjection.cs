using Aethria.Application.Behaviors;
using Aethria.Application.UseCases.Chat.Contracts;
using Aethria.Application.UseCases.Chat.ResourceChat;
using Aethria.Application.UseCases.Resources.GetResourceSelector;
using DispatchR.Extensions;

namespace Aethria.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApiApplicationServices(this IServiceCollection services)
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

    public static IServiceCollection AddMcpApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IValidator<GetResourceSelectorQuery>, GetResourceSelectorQueryValidator>();
        services.AddScoped<IValidator<ResourceChatCommand>, ResourceChatCommandValidator>();

        services.AddDispatchR(options =>
        {
            options.Assemblies.Add(typeof(DependencyInjection).Assembly);
            options.RegisterPipelines = true;
            options.RegisterNotifications = false;
            options.PipelineOrder =
            [
                typeof(ValidationResultPipelineBehavior<,>),
                typeof(ValidationStreamPipelineBehavior<,>)
            ];
            options.IncludeHandlers =
            [
                typeof(GetResourceSelectorQueryHandler),
                typeof(ResourceChatCommandHandler)
            ];
        });

        return services;
    }
}
