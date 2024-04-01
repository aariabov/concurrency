using System.Collections.Concurrent;
using System.Diagnostics;

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
}