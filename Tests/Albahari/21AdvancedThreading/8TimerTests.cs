namespace Tests.Albahari._21AdvancedThreading;

[TestClass]
public class _8TimerTests
{
    [TestMethod]
    public async Task timer_example()
    {
        Timer tmr = new Timer(Tick, "tick...", 0, 1000);
        Console.WriteLine("Press Enter to stop");
        Thread.Sleep(3000);
        tmr.Dispose();

        void Tick(object data)
        {
            Console.WriteLine(data);
        }
    }

    [TestMethod]
    public async Task timer_example1()
    {
        var tmr = new System.Timers.Timer();
        tmr.Interval = 500;
        tmr.Elapsed += tmr_Elapsed;
        tmr.Start();
        Thread.Sleep(2000);
        tmr.Stop();
        Console.WriteLine("Sleep");
        tmr.Start();
        Thread.Sleep(2000);
        tmr.Dispose();

        static void tmr_Elapsed(object sender, EventArgs e)
        {
            Console.WriteLine("Tick");
        }
    }
}
