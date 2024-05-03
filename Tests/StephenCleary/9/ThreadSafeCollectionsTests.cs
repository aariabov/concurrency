using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace Tests._9;

[TestClass]
public class ThreadSafeCollectionsTests
{
    [TestMethod]
    public async Task concurrent_dictionary_example()
    {
        var dictionary = new ConcurrentDictionary<int, string>();
        string newValue = dictionary.AddOrUpdate(
            0, // ключ
            key => "Zero", // делегат, преобразующий ключ в значение, которое будет добавлено
            (key, oldValue) => "Zero"); //  делегат, преобразующий ключ и старое значение в обновленное значение, которое должно быть сохранено в словаре

        dictionary[1] = "One"; // упрощенный синтаксис
        
        bool keyExists = dictionary.TryGetValue(0, out string currentValue);
        Assert.IsTrue(keyExists);
        Assert.AreEqual("Zero", currentValue);
        
        bool keyExisted = dictionary.TryRemove(0, out string removedValue);
        Assert.IsTrue(keyExisted);
        Assert.AreEqual("Zero", removedValue);

    }
    
    [TestMethod]
    public async Task blocking_queue_example()
    {
        BlockingCollection<int> _blockingQueue = new BlockingCollection<int>();

        Trace.WriteLine($"ThreadId: {Thread.CurrentThread.ManagedThreadId}");
        _blockingQueue.Add(7);
        _blockingQueue.Add(13);
        _blockingQueue.CompleteAdding();
        
        await Task.Run(() =>
        {
            Trace.WriteLine($"ThreadId: {Thread.CurrentThread.ManagedThreadId}");
            foreach (int item in _blockingQueue.GetConsumingEnumerable())
            {
                Trace.WriteLine(item);
            }
        });
    }
    
    [TestMethod]
    public async Task blocking_stack_example()
    {
        var _blockingStack = new BlockingCollection<int>(new ConcurrentStack<int>());
        
        // Код-производитель
        Trace.WriteLine($"ThreadId: {Thread.CurrentThread.ManagedThreadId}");
        _blockingStack.Add(7);
        _blockingStack.Add(13);
        _blockingStack.CompleteAdding();
        
        // Код-потребитель
        // Выводит "13", затем "7".
        await Task.Run(() =>
        {
            Trace.WriteLine($"ThreadId: {Thread.CurrentThread.ManagedThreadId}");
            foreach (int item in _blockingStack.GetConsumingEnumerable())
            {
                Trace.WriteLine(item);
            }
        });
    }
    
    [TestMethod]
    public async Task channel_example()
    {
        Channel<int> queue = Channel.CreateUnbounded<int>();
        // Код-производитель
        ChannelWriter<int> writer = queue.Writer;
        await writer.WriteAsync(7);
        await writer.WriteAsync(13);
        writer.Complete();
        
        // Код-потребитель
        ChannelReader<int> reader = queue.Reader;
        await foreach (int value in reader.ReadAllAsync())
            Trace.WriteLine(value);
    }
    
    [TestMethod]
    public async Task buffer_block_example()
    {
        var _asyncQueue = new BufferBlock<int>();
        // Код-производитель.
        await _asyncQueue.SendAsync(7);
        await _asyncQueue.SendAsync(13);
        _asyncQueue.Complete();
        
        // Код-потребитель.
        while (await _asyncQueue.OutputAvailableAsync())
            Trace.WriteLine(await _asyncQueue.ReceiveAsync());
    }
    
    [TestMethod]
    public async Task channel_with_capacity_example()
    {
        Channel<int> queue = Channel.CreateBounded<int>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
            });
        ChannelWriter<int> writer = queue.Writer;
        // Операция записи завершается немедленно.
        await writer.WriteAsync(7);
        
        // Операция записи тоже завершается немедленно.
        // Элемент 7 теряется, если только он не был
        // немедленно извлечен потребителем.
        await writer.WriteAsync(13);
    }
}