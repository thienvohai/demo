using EmailService.Core;

namespace EmailService.Service;

public class BackgroundServiceEventBus : IEventBus
{
    private readonly IEmailMessageTaskQueue taskQueue;
    public BackgroundServiceEventBus(IEmailMessageTaskQueue taskQueue)
    {
        this.taskQueue = taskQueue;
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : BaseEventMessage
    {
        await taskQueue.QueueAsync(message);    
    }
}
