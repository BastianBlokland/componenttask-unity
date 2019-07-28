# ComponentTask-Unity

[![Tests](https://img.shields.io/azure-devops/tests/bastian-blokland/ComponentTask/6/master.svg)](https://dev.azure.com/bastian-blokland/ComponentTask/_build/latest?definitionId=6&branchName=master)
[![GitHub release](https://img.shields.io/github/release/BastianBlokland/componenttask-unity.svg)](https://github.com/BastianBlokland/componenttask-unity/releases/)
![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)

Unity package for running dotnet `Task` and `Task<T>` scoped to Unity components.


### Description
Common problem with using c# `async` methods in Unity is that they have no concept of a component
life-time.
So unlike [Coroutines](https://docs.unity3d.com/ScriptReference/Coroutine.html) when you 'start' a
task and then destroy the component (by loading a new scene for example) the task does not stop.
Most of the time leading to `UnityEngine.MissingReferenceException` when you try to access members
of the component after it has been destroyed on the Unity side. And as a workaround you have to
write ugly '`if (!this) return;`' guards after every await.

This library aims to fix that problem by allowing you to run tasks 'on' your `MonoBehaviour` with very
similar behaviour as Unity's `Coroutines`.


### Usage
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

[More examples](https://bastianblokland.github.io/componenttask-unity/examples.html)


### Documentation
* [Install](https://bastianblokland.github.io/componenttask-unity/install.html)
* [How To](https://bastianblokland.github.io/componenttask-unity/how-to.html)
* [Examples](https://bastianblokland.github.io/componenttask-unity/examples.html)
* [Api Reference](https://bastianblokland.github.io/componenttask-unity/api/index.html)
