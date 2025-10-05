namespace UknfPlatform.Application.Shared.Interfaces;

/// <summary>
/// Service for publishing domain events to message bus
/// (RabbitMQ, Azure Service Bus, or in-memory for development)
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to the message bus
    /// </summary>
    /// <typeparam name="TEvent">Event type</typeparam>
    /// <param name="event">Event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}

