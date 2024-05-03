namespace Tests._3;

public class Sut
{
    public IEnumerable<int> GetValues()
    {
        yield return 10;
        yield return 13;
    }
    
    public async IAsyncEnumerable<int> GetValuesAsync()
    {
        await Task.Delay(10); // Асинхронная работа
        yield return 10;
        await Task.Delay(10); // Другая асинхронная работа
        yield return 13;
    }
    
    public async IAsyncEnumerable<string> GetDataAsync()
    {
        string[] data = { "Tom", "Sam", "Kate", "Alice", "Bob" };
        for (int i = 0; i < data.Length; i++)
        {
            Console.WriteLine($"Получаем {i + 1} элемент");
            await Task.Delay(500);
            yield return data[i];
        }
    }
}