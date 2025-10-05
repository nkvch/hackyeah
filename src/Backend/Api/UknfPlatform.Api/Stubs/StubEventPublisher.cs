using MediatR;
using UknfPlatform.Application.Shared.Interfaces;

namespace UknfPlatform.Api.Stubs;

/// <summary>
/// Event publisher that uses MediatR for in-process domain events.
/// For Story 4.2, this publishes to MediatR which then publishes to RabbitMQ.
/// </summary>
public class StubEventPublisher : IEventPublisher
{
    private readonly IMediator _mediator;
    private readonly ILogger<StubEventPublisher> _logger;

    public StubEventPublisher(IMediator mediator, ILogger<StubEventPublisher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        _logger.LogInformation(
            "Publishing event via MediatR: {EventType}",
            typeof(TEvent).Name);

        // If the event implements INotification, publish via MediatR
        if (@event is INotification notification)
        {
            await _mediator.Publish(notification, cancellationToken);
        }
        else
        {
            _logger.LogWarning(
                "Event {EventType} does not implement INotification, cannot be published",
                typeof(TEvent).Name);
        }
    }
}

