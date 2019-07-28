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


### Documentation:
* [Install](install.md)
* [How To](how-to.md)
* [Examples](examples.md)
* [Api Reference](/api/)


### Repository
[![GitHub issues](https://img.shields.io/github/issues/BastianBlokland/componenttask-unity.svg)](https://GitHub.com/BastianBlokland/componenttask-unity/issues/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](https://github.com/BastianBlokland/componenttask-unity/pulls)


### CI
Basic Azure pipeline for running tests: [Pipeline](https://dev.azure.com/bastian-blokland/ComponentTask/_build?definitionId=6).
