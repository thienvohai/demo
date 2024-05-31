using EmailService.Core;
using Newtonsoft.Json;
using System.Threading.Channels;

namespace EmailService.Service;

public interface IEmailMessageTaskQueue
{
    ValueTask QueueAsync<T>(T workItem) where T : BaseEventMessage;
    ValueTask<T?> DequeueAsync<T>(CancellationToken cancellationToken) where T : BaseEventMessage;
}

public class EmailMesssageTaskQueue : IEmailMessageTaskQueue
{
    private readonly Channel<string> queue;

    public EmailMesssageTaskQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        queue = Channel.CreateBounded<string>(options);
    }


    public async ValueTask QueueAsync<T>(
        T workItem) where T : BaseEventMessage
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }
        var queueItem = JsonConvert.SerializeObject(workItem, Formatting.Indented);
        await queue.Writer.WriteAsync(queueItem);
    }

    public async ValueTask<T?> DequeueAsync<T>(
        CancellationToken cancellationToken) where T : BaseEventMessage
    {
        var queueItem = await queue.Reader.ReadAsync(cancellationToken);
        var workItem = JsonConvert.DeserializeObject<T>(queueItem); 
        return workItem;
    }
}