# How To


### Usage
* Start a 'Task' on your `MonoBehaviour` with `this.StartTask(...)`.
* The task then runs 'on' your `MonoBehaviour` meaning it gets paused when your `MonoBehaviour` is
disabled and it gets stopped when your `MonoBehaviour` gets destroyed.
* This means that inside your task you don't need to worry about being destroyed.
* Any exceptions that happen inside your `Task` are reported to the Unity log, so no silent failures.


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
