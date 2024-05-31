namespace EmailService.Core;

public interface IEventBus
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : BaseEventMessage;
}
