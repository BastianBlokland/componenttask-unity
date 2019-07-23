# ComponentTask-Unity

[![Tests](https://img.shields.io/azure-devops/tests/bastian-blokland/ComponentTask/6/master.svg)](https://dev.azure.com/bastian-blokland/ComponentTask/_build/latest?definitionId=6&branchName=master)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Unity package for running dotnet `Task` and `Task<T>` scoped to Unity components.

## Description
Common problem with using c# `async` methods in Unity is that they have no concept of a component
life-time.
So unlike [Coroutines](https://docs.unity3d.com/ScriptReference/Coroutine.html) when you 'start' a
task and then destroy the component (by loading a new scene for example) the task does not stop.
Most of the time leading to `UnityEngine.MissingReferenceException` when you try to access members
of the component after it has been destroyed on the Unity side. And as a workaround you have to
write ugly '`if (!this) return;`' guards after every await.

This library aims to fix that problem by allowing you to run tasks 'on' your `MonoBehaviour` with very
similar behaviour as Unity's `Coroutines`.

## Installation
Add a reference to this repository to your package dependencies (`Packages/manifest.json`)
```
"dependencies": {
    "com.bastianblokland.componenttask": "https://github.com/BastianBlokland/componenttask-unity.git#v1.0",
    ...
}
```

## Usage
* Start a 'Task' on your `MonoBehaviour` with `this.StartTask(...)`.
* The task then runs 'on' your `MonoBehaviour` meaning it gets paused when your `MonoBehaviour` is
disabled and it gets stopped when your `MonoBehaviour` gets destroyed.
* This means that inside your task you don't need to worry about being destroyed.
* Any exceptions that happen inside your `Task` are reported to the Unity log, so no silent failures.

## Examples
These examples can also be found in the [`.example`](https://github.com/BastianBlokland/componenttask-unity/tree/master/.example) directory.

### Basic example
```c#
using System.Threading.Tasks;
using UnityEngine;

class MyClass : MonoBehaviour
{
    void Start()
    {
        this.StartTask(RunAsync);
    }

    async Task RunAsync()
    {
        while (true)
        {
            Debug.Log("Running...");
            await Task.Yield();
        }
    }
}
```
This example will print `Running...` every frame when the component is enabled and will stop when
the component gets destroyed.

### Awaiting other methods
```c#
using System;
using System.Threading.Tasks;
using UnityEngine;

class MyClass : MonoBehaviour
{
    void Start()
    {
        this.StartTask(RunAsync);
    }

    async Task RunAsync()
    {
        while (true)
        {
            var val = await GetValueAsync();
            Debug.Log($"Got value: '{val}'");
        }
    }

    async Task<int> GetValueAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        return Time.frameCount;
    }
}
```
When you `await` other methods they automatically belong to the same scope as the task that starts
them. So in this example `GetValueAsync` also runs as part of the `MyClass` scope and stop when
the component is destroyed.

### Exposing a method that produces a value
But what if you want to expose a api that produces a value, what happens to the task once your component
gets destroyed.
```c#
using System;
using System.Threading.Tasks;
using UnityEngine;

class Producer : MonoBehaviour
{
    public Task<int> GetValueAsync() => this.StartTask(ProduceValueAsync);

    async Task<int> ProduceValueAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        return Time.frameCount;
    }
}
```
The `Task` (or `Task<T>`) returned out of  `This.StartTask(...)` properly goes into a 'cancelled'
state when the component is destroyed. Which means that when you await that task you will get a
`TaskCanceledException` that you can handle.

On the receiving side:
```c#
using System.Threading.Tasks;
using UnityEngine;

class Consumer : MonoBehaviour
{
    [SerializeField] private Producer producer;

    void Start()
    {
        this.StartTask(RunAsync);
    }

    async Task RunAsync()
    {
        try
        {
            var val = await producer.GetValueAsync();
            Debug.Log($"Got value: '{val}'");
        }
        catch (TaskCanceledException)
        {
            Debug.Log("The producer was destroyed before producing the result");
        }
    }
}
```
If you don't catch the exception then the exception is logged to the unity log and your task will
go to a faulted state. But the nice thing is that if both components have the same lifetime
(destroyed at the same time) then there is no problem (and you won't get any exceptions).

### Avoiding closures
To avoid having to capture closures you can pass an argument into you task using `this.StartTask(...)`.
```c#
using System;
using System.Threading.Tasks;
using UnityEngine;

class MyClass : MonoBehaviour
{
    void Start()
    {
        var delay = 1;
        this.StartTask(WaitAndDoSomethingAsync, delay);
    }

    async Task WaitAndDoSomethingAsync(int secondsDelay)
    {
        await Task.Delay(TimeSpan.FromSeconds(secondsDelay));
        Debug.Log("Doing something");
    }
}
```
Only one argument is supported but with the 'new' tuples in c# 7 there is a nice (and efficient) workaround:
```c#
using System;
using System.Threading.Tasks;
using UnityEngine;

class MyClassWithValueTuple : MonoBehaviour
{
    void Start()
    {
        this.StartTask(WaitAndLog, (secondsDelay: 1, message: "Hello World"));
    }

    async Task WaitAndLog((int secondsDelay, string message) input)
    {
        await Task.Delay(TimeSpan.FromSeconds(input.secondsDelay));
        Debug.Log(input.message);
    }
}
```

### Cancelling external work
To make it easier to cancel external work when your component is destroyed `this.StartTask(...)`
optionally gives you a `CancellationToken` to give to external api's.
```c#
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

class MyClass : MonoBehaviour
{
    void Start()
    {
        var url = "https://github.com/BastianBlokland/componenttask-unity";
        this.StartTask(DownloadTextAsync, url);
    }

    async Task DownloadTextAsync(string url, CancellationToken cancelToken)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url, cancelToken);
            var responseText = await response.Content.ReadAsStringAsync();
            Debug.Log($"Text: '{responseText}'");
        }
    }
}
```
Giving the `CancellationToken` here will make sure that the web-request is actually aborted when
this component is destroyed.

### Running expensive blocking work on a background thread
Something that the `Task` based model make very easy is interacting with code that runs on a
different thread. You can for example run your expensive blocking code in a background thread and
await a `Task` handle to it.
```c#
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

class MyClass : MonoBehaviour
{
    void Start()
    {
        this.StartTask(RunAsync);
    }

    async Task RunAsync()
    {
        var result = await Task.Run(VeryExpensiveBlockingCode);
        Debug.Log($"Got value: '{result}'");
    }

    int VeryExpensiveBlockingCode()
    {
        Thread.Sleep(TimeSpan.FromSeconds(5));
        return 42;
    }
}
```
Even though `VeryExpensiveBlockingCode` blocks for 5 seconds because we run it on a background-thread
(with [Task.Run](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run)) the unity-thread stays responsive.

## Apis
So far we've been starting tasks using extension methods on `UnityEngine.Component`:
```c#
Task StartTask(this Component component, Func<Task> taskCreator);

Task StartTask(this Component component, Func<CancellationToken, Task> taskCreator);

Task StartTask<TIn>(this Component component, Func<TIn, Task> taskCreator, TIn data);

Task StartTask<TIn>(this Component component, Func<TIn, CancellationToken, Task> taskCreator, TIn data);

Task<TOut> StartTask<TOut>(this Component component, Func<Task<TOut>> taskCreator);

Task<TOut> StartTask<TOut>(this Component component, Func<CancellationToken, Task<TOut>> taskCreator);

Task<TOut> StartTask<TIn, TOut>(this Component component, Func<TIn, Task<TOut>> taskCreator, TIn data);

Task<TOut> StartTask<TIn, TOut>(this Component component, Func<TIn, CancellationToken, Task<TOut>> taskCreator, TIn data);
```
Alternatively you can manually get a `ITaskRunner` for a component:
```c#
ITaskRunner GetTaskRunner(this Component component);
```
If a runner was already created for that component then the existing one is reused.

If you want to scope tasks to a `UnityEngine.GameObject` but not to a particular component you can use:
```c#
ITaskRunner CreateTaskRunner(this GameObject gameObject);
```

## Warning
* NEVER hand out tasks that are created from within your scope, as those tasks may stop running and
the caller would never know. ALWAYS use `this.StartTask(...)` when handing out a task outside of your scope.
