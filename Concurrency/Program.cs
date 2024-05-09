// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");
await foreach (var i in Test())
{
    Console.WriteLine(i);
}

async IAsyncEnumerable<int> Test()
{
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(100);
        yield return i;
    }
}
