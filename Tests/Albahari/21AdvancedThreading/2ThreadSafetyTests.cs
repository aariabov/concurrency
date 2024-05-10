using System.Diagnostics;

namespace Tests.Albahari._21AdvancedThreading;

[TestClass]
public class _2ThreadSafetyTests
{
    [TestMethod]
    public async Task list_unsafe_problem()
    {
        var watch = Stopwatch.StartNew();
        List<string> list = new List<string>();
        for (int i = 0; i < 100000; i++)
        {
            new Thread(AddItem).Start();
        }
        Console.WriteLine($"Items {list.Count}, time {watch.ElapsedMilliseconds}");

        void AddItem()
        {
            list.Add("Item " + list.Count);
        }
    }

    [TestMethod]
    public async Task list_safe_lock()
    {
        var watch = Stopwatch.StartNew();
        List<string> list = new List<string>();
        for (int i = 0; i < 100000; i++)
        {
            new Thread(AddItem).Start();
        }
        Console.WriteLine($"Items {list.Count}, time {watch.ElapsedMilliseconds}");

        void AddItem()
        {
            lock (list)
            {
                list.Add("Item " + list.Count);
            }
        }
    }

    [TestMethod]
    public async Task cache_unsafe()
    {
        var watch = Stopwatch.StartNew();
        Dictionary<int, User> users = new Dictionary<int, User>();
        User GetUser(int id)
        {
            User u = null;

            if (users.TryGetValue(id, out u))
                return u;

            u = RetrieveUser(id);
            users[id] = u;
            return u;
        }

        User RetrieveUser(int id)
        {
            return new User { ID = id };
        }

        // будет либо исключение, либо неправильное число
        await Task.WhenAll(Enumerable.Range(0, 100000).Select(i => Task.Run(() => GetUser(i))));
        Console.WriteLine($"Users count {users.Count}, time {watch.ElapsedMilliseconds}");
    }

    [TestMethod]
    public async Task cache_safe_lock()
    {
        var watch = Stopwatch.StartNew();
        Dictionary<int, User> users = new Dictionary<int, User>();
        User GetUser(int id)
        {
            User u = null;
            lock (users)
                if (users.TryGetValue(id, out u))
                    return u;

            u = RetrieveUser(id);
            lock (users)
                users[id] = u;
            return u;
        }

        User RetrieveUser(int id)
        {
            return new User { ID = id };
        }

        await Task.WhenAll(Enumerable.Range(0, 100000).Select(i => Task.Run(() => GetUser(i))));
        Console.WriteLine($"Users count {users.Count}, time {watch.ElapsedMilliseconds}");
    }

    [TestMethod]
    public async Task cache_safe_lock_method()
    {
        var watch = Stopwatch.StartNew();
        Dictionary<int, User> users = new Dictionary<int, User>();
        User GetUser(int id)
        {
            User u = null;
            lock (users)
                if (users.TryGetValue(id, out u))
                    return u;

            u = RetrieveUser(id);
            lock (users)
                users[id] = u;
            return u;
        }

        User RetrieveUser(int id)
        {
            return new User { ID = id };
        }

        await Task.WhenAll(Enumerable.Range(0, 100000).Select(i => Task.Run(() =>
        {
            lock (users)
                GetUser(i);
        })));
        Console.WriteLine($"Users count {users.Count}, time {watch.ElapsedMilliseconds}");
    }

    [TestMethod]
    public async Task cache_tasks_safe_lock()
    {
        var watch = Stopwatch.StartNew();
        Dictionary <int, Task<User>> _userTasks = new Dictionary <int, Task<User>>();
        Task<User> GetUserAsync (int id)
        {
            lock (_userTasks)
                if (_userTasks.TryGetValue (id, out var userTask))
                    return userTask;
                else
                    return _userTasks [id] = Task.Run (() => RetrieveUser (id));
        }

        User RetrieveUser(int id)
        {
            return new User { ID = id };
        }

        await Task.WhenAll(Enumerable.Range(0, 100000).Select(i => Task.Run(() => GetUserAsync(i))));
        Console.WriteLine($"Users count {_userTasks.Count}, time {watch.ElapsedMilliseconds}");
    }

    class User
    {
        public int ID;
    }
}
