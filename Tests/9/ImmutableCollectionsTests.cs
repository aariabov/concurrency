using System.Collections.Immutable;
using System.Diagnostics;

namespace Tests._9;

[TestClass]
public class ImmutableCollectionsTests
{
    [TestMethod]
    public async Task immutable_stack_example()
    {
        ImmutableStack<int> stack = ImmutableStack<int>.Empty;
        stack = stack.Push(13); // создается новый стек
        stack = stack.Push(7); // создается новый стек

        foreach (int item in stack)
        {
            Trace.WriteLine(item);
        }

        stack = stack.Pop(out var lastItem); // создается новый стек
        Assert.AreEqual(7, lastItem);
    }
    
    [TestMethod]
    public async Task immutable_stack_example1()
    {
        ImmutableStack<int> stack = ImmutableStack<int>.Empty;
        stack = stack.Push(13);
        ImmutableStack<int> biggerStack = stack.Push(7);
        // стеки используют общую память для елемента 13

        // Выводит "7", затем "13"
        foreach (int item in biggerStack)
        {
            Trace.WriteLine(item);
        }
        
        // Выводит только "13".
        foreach (int item in stack)
        {
            Trace.WriteLine(item);
        }
        
        Assert.AreEqual(1, stack.Count());
        Assert.AreEqual(2, biggerStack.Count());
    }
    
    [TestMethod]
    public async Task immutable_queue_example()
    {
        ImmutableQueue<int> queue = ImmutableQueue<int>.Empty;
        queue = queue.Enqueue(13);
        queue = queue.Enqueue(7);
        
        foreach (int item in queue)
        {
            Trace.WriteLine(item);
        }

        queue = queue.Dequeue(out var nextItem);
        Assert.AreEqual(13, nextItem);
    }
    
    [TestMethod]
    public async Task immutable_list_example()
    {
        ImmutableList<int> list = ImmutableList<int>.Empty;
        list = list.Insert(0, 13);
        list = list.Insert(0, 7);
        foreach (int item in list)
        {
            Trace.WriteLine(item);
        }
        list = list.RemoveAt(1);
        
        Assert.AreEqual(1, list.Count());
        Assert.AreEqual(7, list.First());
    }
    
    [TestMethod]
    public async Task immutable_hash_set_example()
    {
        ImmutableHashSet<int> hashSet = ImmutableHashSet<int>.Empty;
        hashSet = hashSet.Add(13);
        hashSet = hashSet.Add(7);
        
        // Выводит "7" и "13" в непредсказуемом порядке.
        foreach (int item in hashSet)
        {
            Trace.WriteLine(item);
        }
        hashSet = hashSet.Remove(7);
        
        Assert.AreEqual(1, hashSet.Count());
        Assert.AreEqual(13, hashSet.First());
    }
    
    [TestMethod]
    public async Task immutable_sorted_set_example()
    {
        ImmutableSortedSet<int> sortedSet = ImmutableSortedSet<int>.Empty;
        sortedSet = sortedSet.Add(13);
        sortedSet = sortedSet.Add(7);
        
        // Выводит "7", затем "13".
        foreach (int item in sortedSet)
        {
            Trace.WriteLine(item);
        }
        int smallestItem = sortedSet[0];
        Assert.AreEqual(7, smallestItem);
        
        sortedSet = sortedSet.Remove(7);
        smallestItem = sortedSet[0];
        Assert.AreEqual(13, smallestItem);
    }
    
    [TestMethod]
    public async Task immutable_dictionary_example()
    {
        ImmutableDictionary<int, string> dictionary = ImmutableDictionary<int, string>.Empty;
        dictionary = dictionary.Add(10, "Ten");
        dictionary = dictionary.Add(21, "Twenty-One");
        dictionary = dictionary.SetItem(10, "Diez"); // нельзя dictionary[10] = "Diez"
        
        // Выводит "10Diez" и "21Twenty-One" в непредсказуемом порядке.
        foreach (KeyValuePair<int, string> item in dictionary)
        {
            Trace.WriteLine(item.Key + item.Value);
        }
        Assert.AreEqual(2, dictionary.Count);
        
        dictionary = dictionary.Remove(21);
        Assert.AreEqual(1, dictionary.Count);
    }
    
    [TestMethod]
    public async Task immutable_sorted_dictionary_example()
    {
        var sortedDictionary = ImmutableSortedDictionary<int, string>.Empty;
        sortedDictionary = sortedDictionary.Add(10, "Ten");
        sortedDictionary = sortedDictionary.Add(21, "Twenty-One");
        sortedDictionary = sortedDictionary.SetItem(10, "Diez");
        
        // Выводит "10Diez", затем "21Twenty-One".
        foreach (KeyValuePair<int, string> item in sortedDictionary)
        {
            Trace.WriteLine(item.Key + item.Value);
        }
        Assert.AreEqual(2, sortedDictionary.Count);
        Assert.AreEqual(10, sortedDictionary.First().Key);
        
        sortedDictionary = sortedDictionary.Remove(21);
        Assert.AreEqual(1, sortedDictionary.Count);
        Assert.AreEqual(21, sortedDictionary.First().Key);
    }
}