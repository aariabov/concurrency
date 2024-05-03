using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace Tests._8;

public static class DataflowExtensions
{
    public static bool TryReceiveItem<T>(this ISourceBlock<T> block, out T value)
    {
        if (block is IReceivableSourceBlock<T> receivableSourceBlock)
        {
            return receivableSourceBlock.TryReceive(out value);
        }
        
        try
        {
            value = block.Receive(TimeSpan.Zero);
            return true;
        }
        catch (TimeoutException)
        {
            // На данный момент доступного элемента нет.
            value = default;
            return false;
        }
        catch (InvalidOperationException)
        {
            // Блок завершен, элементов больше нет.
            value = default;
            return false;
        }
    }
    public static async IAsyncEnumerable<T> ReceiveAllAsync<T>(this ISourceBlock<T> block,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await block.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
        {
            while (block.TryReceiveItem(out var value))
            {
                yield return value;
            }
        }
    }
    
    public static async Task WriteToBlockAsync<T>(this IAsyncEnumerable<T> enumerable,
        ITargetBlock<T> block, CancellationToken token = default)
    {
        try
        {
            await foreach (var item in enumerable.WithCancellation(token).ConfigureAwait(false))
            {
                await block.SendAsync(item, token).ConfigureAwait(false);
            }
            block.Complete();
        }
        catch (Exception ex)
        {
            block.Fault(ex);
        }
    }
    
    public static async IAsyncEnumerable<T> ToCustomAsyncEnumerable<T>(this IObservable<T> observable)
    {
        Channel<T> buffer = Channel.CreateUnbounded<T>();
        using (observable.Subscribe(
                   value => buffer.Writer.TryWrite(value),
                   error => buffer.Writer.Complete(error),
                   () => buffer.Writer.Complete()))
        {
            await foreach (T item in buffer.Reader.ReadAllAsync())
                yield return item;
        }
    }
    
    // ПРЕДУПРЕЖДЕНИЕ: возможна потеря элементов; см. обсуждение!
    public static async IAsyncEnumerable<T> ToBufferAsyncEnumerable<T>(this IObservable<T> observable, int bufferSize)
    {
        var bufferOptions = new BoundedChannelOptions(bufferSize)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        };
        Channel<T> buffer = Channel.CreateBounded<T>(bufferOptions);
        using (observable.Subscribe(
                   value => buffer.Writer.TryWrite(value),
                   error => buffer.Writer.Complete(error),
                   () => buffer.Writer.Complete()))
        {
            await foreach (T item in buffer.Reader.ReadAllAsync())
                yield return item;
        }
    }
}