namespace Tests.ShaktiTanwar._5SyncPrimitives;

[TestClass]
public class MemoryBarriersTests
{
    static int a = 1, b = 2, c = 0;
    
    [TestMethod]
    public async Task interlocked_memory_barrier_process_wide()
    {
        b = c;
        Interlocked.MemoryBarrierProcessWide();
        a = 1;
    }
    
    [TestMethod]
    public async Task interlocked_memory_barrier()
    {
        b = c;
        Interlocked.MemoryBarrier();
        a = 1;
    }
    
    [TestMethod]
    public async Task thread_memory_barrier()
    {
        b = c;
        Thread.MemoryBarrier();
        a = 1;
    }
}
