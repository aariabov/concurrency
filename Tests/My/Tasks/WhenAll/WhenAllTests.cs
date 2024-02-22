namespace Tests.My.Tasks.WhenAll;

[TestClass]
public class WhenAllTests
{
    [TestMethod]
    public async Task when_all_sum()
    {
        var sut = new SutService();

        var result = await sut.WhenAllSum(1, 3);
        Assert.AreEqual(6, result);
    }
    
    [TestMethod]
    public async Task when_all_first_exception()
    {
        var sut = new SutService();

        var func = () => sut.WhenAllFirstException(1, 3);
        await Assert.ThrowsExceptionAsync<TimeoutException>(func);
    }
    
    [TestMethod]
    public async Task when_all_catch_all_exceptions()
    {
        var sut = new SutService();

        var func = () => sut.WhenAllCatchAllExceptions(1, 3);
        await Assert.ThrowsExceptionAsync<AggregateException>(func);
    }
}