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
        services.AddScoped<IMediator, Mediator>();

        services.AddScoped<IValidator<GetResourceSelectorQuery>, GetResourceSelectorQueryValidator>();
        services.AddScoped<IValidator<ResourceChatCommand>, ResourceChatCommandValidator>();

        services.AddScoped<GetResourceSelectorQueryHandler>();
        services.AddScoped<ResourceChatCommandHandler>();

        services.AddScoped<IRequestHandler<GetResourceSelectorQuery, ValueTask<Result<GetResourceSelectorResponse>>>>(
            sp => CreateValidatedHandler<GetResourceSelectorQuery, Result<GetResourceSelectorResponse>>(
                sp,
                sp.GetRequiredService<GetResourceSelectorQueryHandler>()));

        services.AddScoped<IStreamRequestHandler<ResourceChatCommand, Result<ChatStreamResponse>>>(
            sp => CreateValidatedStreamHandler<ResourceChatCommand, Result<ChatStreamResponse>>(
                sp,
                sp.GetRequiredService<ResourceChatCommandHandler>()));

        return services;
    }

    private static IRequestHandler<TRequest, ValueTask<TResponse>> CreateValidatedHandler<TRequest, TResponse>(
        IServiceProvider serviceProvider,
        IRequestHandler<TRequest, ValueTask<TResponse>> next)
        where TRequest : class, IRequest<TRequest, ValueTask<TResponse>>
        where TResponse : ResultBase<TResponse>, new()
    {
        var pipeline = ActivatorUtilities.CreateInstance<ValidationResultPipelineBehavior<TRequest, TResponse>>(serviceProvider);
        pipeline.NextPipeline = next;
        return pipeline;
    }

    private static IStreamRequestHandler<TRequest, TResponse> CreateValidatedStreamHandler<TRequest, TResponse>(
        IServiceProvider serviceProvider,
        IStreamRequestHandler<TRequest, TResponse> next)
        where TRequest : class, IStreamRequest<TRequest, TResponse>
        where TResponse : ResultBase<TResponse>, new()
    {
        var pipeline = ActivatorUtilities.CreateInstance<ValidationStreamPipelineBehavior<TRequest, TResponse>>(serviceProvider);
        pipeline.NextPipeline = next;
        return pipeline;
    }
}
