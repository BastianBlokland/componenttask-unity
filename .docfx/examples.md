# Examples


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
them. So in this example `GetValueAsync` also runs as part of the `MyClass` scope and stops when
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


### Caching a task-runner.
If you are going to start many tasks you can also create a ITaskRunner on a gameobject and cache
a reference to it. That runner will remain valid as long as that gameobject is still alive.
```c#
using System.Threading.Tasks;
using UnityEngine;
using ComponentTask;

class MyClass : MonoBehaviour
{
    private ITaskRunner runner;

    void Start()
    {
        this.runner = this.gameObject.CreateTaskRunner();
    }

    void Update()
    {
        this.runner.StartTask(this.WaitAndLogAsync);
    }

    async Task WaitAndLogAsync()
    {
        await Task.Yield();
        Debug.Log("Running");
    }
}
```


### Custom LocalTaskRunner.
If you dont to scope you tasks to Unity Components but control the update ticks yourself you can
manually create a 'LocalTaskRunner' and control its ticks yourself.
```c#
using System;
using System.Threading.Tasks;
using UnityEngine;
using ComponentTask;

class MyClass : MonoBehaviour, IExceptionHandler
{
    [SerializeField] private bool isPaused;

    private LocalTaskRunner runner;

    void Start()
    {
        this.runner = new LocalTaskRunner(exceptionHandler: this);
        this.runner.StartTask(this.RunAsync);
    }

    void Update()
    {
        if (!this.isPaused)
            this.runner.Execute();
    }

    void OnDestroy()
    {
        this.runner.Dispose();
    }

    async Task RunAsync()
    {
        while (true)
        {
            Debug.Log("Running");
            await Task.Yield();
        }
    }

    void IExceptionHandler.Handle(Exception exception)
    {
        Debug.Log($"Exception occurred: '{exception.Message}'");
    }
}
```


### OnGUI tasks using custom LocalTaskRunner.
Because with a custom LocalTaskRunner you control when tasks are updated you could implement tasks
that run during `OnGUI` to draw ui.
```c#
using System;
using System.Threading.Tasks;
using UnityEngine;
using ComponentTask;

class MyClass : MonoBehaviour, IExceptionHandler
{
    private LocalTaskRunner guiTaskRunner;

    void Start()
    {
        this.guiTaskRunner = new LocalTaskRunner(exceptionHandler: this);
        this.guiTaskRunner.StartTask(this.DrawUIAsync);
    }

    void OnGUI()
    {
        this.guiTaskRunner.Execute();
    }

    void OnDestroy()
    {
        this.guiTaskRunner.Dispose();
    }

    async Task DrawUIAsync()
    {
        while (true)
        {
            await Task.Yield();
            GUI.Label(new Rect(0f, 0f, 100f, 100f), "Drawn from a task :)");
        }
    }

    void IExceptionHandler.Handle(Exception exception)
    {
        Debug.Log($"Exception occurred: '{exception.Message}'");
    }
}
```
Using a similar pattern your can make tasks task run during `FixedUpdate` to interact with Unity's
physics world for example.
