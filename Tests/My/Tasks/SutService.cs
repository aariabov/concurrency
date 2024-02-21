﻿using System.Net;
using System.Net.Sockets;

namespace Tests.My.Tasks;

public class SutService
{
    public int Method1()
    {
        Console.WriteLine($"{nameof(Method1)}: {Thread.CurrentThread.ManagedThreadId}");
        return Thread.CurrentThread.ManagedThreadId;
    }
    
    public int Method2()
    {
        Console.WriteLine($"{nameof(Method2)}: {Thread.CurrentThread.ManagedThreadId}");
        return Thread.CurrentThread.ManagedThreadId;
    }

    public Task MyDelay(int milliseconds)
    {
        TaskCompletionSource taskCompletionSource = new TaskCompletionSource();
        Thread.Sleep(milliseconds);
        // говорим, что асинхронный метод завершился,
        // если его не вызвать, то метод будет работать бесконечно
        taskCompletionSource.SetResult();
        return taskCompletionSource.Task;
    }

    public async Task TaskRunDelay(int milliseconds)
    {
        await Task.Run(() =>
        {
            Console.WriteLine($"TaskRunDelay: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(milliseconds);
        });
    }

    public Task TimerDelay(int milliseconds)
    {
        var tcs = new TaskCompletionSource<object>();
        var timer = new Timer(_ =>
        {
            Console.WriteLine($"MyDelay1 SetResult: {Thread.CurrentThread.ManagedThreadId}");
            tcs.SetResult(null);
        }, null, milliseconds, Timeout.Infinite);
        tcs.Task.ContinueWith(_ =>
        {
            Console.WriteLine($"MyDelay1 ContinueWith: {Thread.CurrentThread.ManagedThreadId}");
            timer.Dispose();
        });
        Console.WriteLine($"MyDelay1 return: {Thread.CurrentThread.ManagedThreadId}");
        return tcs.Task;
    }
    
    public Task<byte[]> DownloadDataAsync(WebClient webClient, string uri)
    {
        var taskCompletionSource = new TaskCompletionSource<byte[]>();
        // используем устаревший способ
        webClient.DownloadDataCompleted += (sender, args) =>
        {
            // возвращаем результат современным способом
            taskCompletionSource.SetResult(args.Result);
        };
        webClient.DownloadDataAsync(new Uri(uri));
        
        return taskCompletionSource.Task;
    }
}