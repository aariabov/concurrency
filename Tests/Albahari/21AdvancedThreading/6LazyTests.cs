namespace Tests.Albahari._21AdvancedThreading;

[TestClass]
public class _6LazyTests
{
    [TestMethod]
    public async Task manual_lazy_unsafe()
    {
        var expensive = new FooUnsafe().Expensive;
    }

    class FooUnsafe
    {
        Expensive _expensive;
        public Expensive Expensive // Lazily instantiate Expensive
        {
            get
            {
                if (_expensive == null)
                    _expensive = new Expensive();
                return _expensive;
            }
        }
    }

    class Expensive { /* Suppose this is expensive to construct */
    }

    [TestMethod]
    public async Task manual_lazy_safe()
    {
        var expensive = new FooLock().Expensive;
    }

    class FooLock
    {
        Expensive _expensive;
        readonly object _expenseLock = new object();

        public Expensive Expensive
        {
            get
            {
                lock (_expenseLock)
                {
                    if (_expensive == null)
                        _expensive = new Expensive();
                    return _expensive;
                }
            }
        }
    }

    [TestMethod]
    public async Task lazy()
    {
        var expensive = new FooLazy().Expensive;
    }

    class FooLazy
    {
        Lazy<Expensive> _expensive = new(() => new Expensive(), true);
        public Expensive Expensive => _expensive.Value;
    }

    [TestMethod]
    public async Task lazy_initializer()
    {
        var expensive = new FooInitializer().Expensive;
    }
    
    class FooInitializer
    {
        Expensive _expensive;
        public Expensive Expensive
        {                    // Implement double-checked locking
            get 
            { 
                LazyInitializer.EnsureInitialized (ref _expensive, () => new Expensive());
                return _expensive;
            }
        }
    }
}
