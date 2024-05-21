namespace Tests.Albahari._21AdvancedThreading;

[TestClass]
public class _5BarrierTests
{
    [TestMethod]
    public async Task barrier_example()
    {
        var barrier = new Barrier(3);

        new Thread(Speak).Start();
        new Thread(Speak).Start();
        new Thread(Speak).Start();

        void Speak()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.Write(i + " ");
                barrier.SignalAndWait();
            }
        }
    }
    
    [TestMethod]
    public async Task barrier_example1()
    {
        var barrier = new Barrier (3, barrier => Console.WriteLine());
        new Thread (Speak).Start();
        new Thread (Speak).Start();
        new Thread (Speak).Start();
        
        void Speak()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.Write (i + " ");
                barrier.SignalAndWait();
            }
        }
    }
}
