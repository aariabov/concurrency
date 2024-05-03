using System.Threading.Tasks.Dataflow;

namespace Tests._5;

[TestClass]
public class DataflowTests
{
    [TestMethod]
    public async Task transform_block_example()
    {
        var multiplyBlock = new TransformBlock<int, int>(item => item * 2);

        multiplyBlock.Post(2);
        Assert.AreEqual(4, multiplyBlock.Receive());

        multiplyBlock.Post(3);
        Assert.AreEqual(6, multiplyBlock.Receive());
    }
    
    [TestMethod]
    public async Task transform_block_link_to_example()
    {
        var multiplyBlock = new TransformBlock<int, int>(item => item * 2);
        var subtractBlock = new TransformBlock<int, int>(item => item - 2);
        multiplyBlock.LinkTo(subtractBlock);

        multiplyBlock.Post(2);
        Assert.AreEqual(2, subtractBlock.Receive());

        multiplyBlock.Post(3);
        Assert.AreEqual(4, subtractBlock.Receive());
    }
    
    [TestMethod]
    public async Task dataflow_with_exception()
    {
        var block = new TransformBlock<int, int>(item =>
        {
            if (item == 1)
            {
                throw new InvalidOperationException("Blech.");
            }
            
            return item * 2;
        });
        block.Post(1);
        block.Post(2);
        
        var func = () => block.Completion;
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(func);
    }
    
    [TestMethod]
    public async Task dataflow_with_aggregate_exception()
    {
        var multiplyBlock = new TransformBlock<int, int>(item =>
        {
            if (item == 1)
                throw new InvalidOperationException("Blech.");
            return item * 2;
        });
        var subtractBlock = new TransformBlock<int, int>(item => item - 2);
        multiplyBlock.LinkTo(subtractBlock, new DataflowLinkOptions { PropagateCompletion = true });
        multiplyBlock.Post(1);
        
        var func = () => subtractBlock.Completion;
        await Assert.ThrowsExceptionAsync<AggregateException>(func);
    }
    
    [TestMethod]
    public async Task link_to_with_parallelism()
    {
        var multiplyBlock = new TransformBlock<int, int>(
            item =>
            {
                Console.WriteLine($"MultiplyBlock: {Thread.CurrentThread.ManagedThreadId}");
                return item * 2;
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded
            });
        var subtractBlock = new TransformBlock<int, int>(item =>
        {
            Console.WriteLine($"SubtractBlock: {Thread.CurrentThread.ManagedThreadId}");
            return item - 2;
        });
        multiplyBlock.LinkTo(subtractBlock);

        multiplyBlock.Post(2);
        Assert.AreEqual(2, subtractBlock.Receive());

        multiplyBlock.Post(3);
        Assert.AreEqual(4, subtractBlock.Receive());
    }
    
    [TestMethod]
    public async Task custom_block()
    {
        var customBlock = CreateMyCustomBlock();

        customBlock.Post(2);
        Assert.AreEqual(3, customBlock.Receive());
    }
    
    IPropagatorBlock<int, int> CreateMyCustomBlock()
    {
        var multiplyBlock = new TransformBlock<int, int>(item => item * 2);
        var addBlock = new TransformBlock<int, int>(item => item + 2);
        var divideBlock = new TransformBlock<int, int>(item => item / 2);
        var flowCompletion = new DataflowLinkOptions { PropagateCompletion = true };
        multiplyBlock.LinkTo(addBlock, flowCompletion);
        addBlock.LinkTo(divideBlock, flowCompletion);
        return DataflowBlock.Encapsulate(multiplyBlock, divideBlock);
    }
}