# SimpleDependencyLocator

`SimpleDependencyLocator` is a very small Unity `ServiceLocator` package with two main goals:

- resolve `services` registered in a configuration asset;
- expose `services` and `signals` in annotated classes through `InjectAttribute` and source generators.

The project is split into three parts:

- runtime code for service and signal resolution;
- editor tooling for service configuration and Inspector integration;
- a source generator that creates the injection accessors automatically.

## Overview

The main flow works like this:

1. You create interfaces that inherit from `IService`.
2. You register the concrete implementation in `ServiceContainerConfig`.
3. You mark a `partial` class with `[Inject(...)]`.
4. The source generator creates access members for the requested types.
5. At runtime, services are resolved through `ServiceLocator` and signals through `SignalLocator`.

## Main namespaces and types

- Runtime namespace: `Injector`
- Global service locator: `Locator.Instance.ServiceLocator`
- Global signal locator: `Locator.Instance.SignalLocator`
- Service marker interface: `IService`
- Injection attribute: `InjectAttribute`

## How services work

### 1. Create the service interface

```csharp
using Injector;

public interface IGameSettingsService : IService
{
    float MasterVolume { get; set; }
    void Save();
}
```

### 2. Create the concrete implementation

```csharp
using System;
using UnityEngine;
using Injector;

[Serializable]
public sealed class GameSettingsService : IGameSettingsService
{
    [field: SerializeField] public float MasterVolume { get; set; } = 1f;

    public void Save()
    {
        Debug.Log($"Saving volume: {MasterVolume}");
    }
}
```

### 3. Register it in `ServiceContainerConfig`

The locator loads an asset named `ServiceContainerConfig` from `Resources`:

`Assets/Resources/ServiceContainerConfig.asset`

You can create or edit this asset through the menu:

`Simple Dependency Locator/Service Container Config`

Each entry accepts a serialized object implementing `IService`. During serialization, the asset automatically stores:

- the concrete type;
- every implemented interface except the base `IService` interface.

That allows resolution by either interface or concrete type.

### 4. Resolve manually when needed

```csharp
var settings = ServiceLocator.Instance.Resolve<IGameSettingsService>();
settings.Save();
```

Also available:

- `TryResolve<T>(out T service)`
- `Bind<T>(T service)`
- `Unbind<T>()`
- `ResolveToRef<T>(ref T service)`

## How attribute-based injection works

The current attribute is applied to the class, not to fields:

```csharp
[Inject(typeof(IGameSettingsService))]
public partial class SettingsPresenter : MonoBehaviour
{
    private void Start()
    {
        GameSettingsService.MasterVolume = 0.75f;
        GameSettingsService.Save();
    }
}
```

### Important rules

- The class must be `partial`.
- The types passed to `InjectAttribute` are inspected by the source generator.
- For each `service`, the generator creates a private property inside the class.
- For each `signal`, the generator creates a public property that accesses `SignalLocator`.

### What gets generated for services

If you declare:

```csharp
[Inject(typeof(IGameSettingsService))]
public partial class SettingsPresenter : MonoBehaviour
{
}
```

the generator creates something equivalent to:

```csharp
private SingletonGameSettingsServiceInjectionField GameSettingsService { get; }
```

That proxy forwards calls to:

```csharp
ServiceLocator.Instance.Resolve<IGameSettingsService>()
```

The generated property name follows this rule:

- take the type name;
- if it starts with `I`, remove the leading `I`;
- use the result as the property name.

Examples:

- `IGameSettingsService` becomes `GameSettingsService`
- `IAudio` becomes `Audio`

## How signals work

Signals live in memory inside `SignalLocator` and are created on demand.

### 1. Create a signal

Without payload:

```csharp
using System;
using Injector;

[Serializable]
public sealed class PlayerDiedSignal : Signal
{
}
```

With payload:

```csharp
using System;
using Injector;

[Serializable]
public sealed class ScoreChangedSignal : Signal<int>
{
}
```

There are also convenience base types for common payloads:

- `IntSignal`
- `FloatSignal`
- `StringSignal`
- `BoolSignal`

### 2. Use signals directly through the locator

```csharp
SignalLocator.Instance.Subscribe<PlayerDiedSignal>(OnPlayerDied);
SignalLocator.Instance.Dispatch<PlayerDiedSignal>();

SignalLocator.Instance.Subscribe<ScoreChangedSignal, int>(OnScoreChanged);
SignalLocator.Instance.Dispatch<ScoreChangedSignal, int>(42);
```

### 3. Inject signals through `[Inject]`

```csharp
[Inject(typeof(ScoreChangedSignal))]
public partial class HUDPresenter : MonoBehaviour
{
    private void Start()
    {
        ScoreChangedSignal.Subscribe(OnScoreChanged);
    }

    private void OnDestroy()
    {
        ScoreChangedSignal.Unsubscribe(OnScoreChanged);
    }

    private void OnScoreChanged(int score)
    {
        Debug.Log($"Score: {score}");
    }
}
```

For signals, the generator creates a public property using the exact signal type name:

```csharp
public ScoreChangedSignal ScoreChangedSignal
{
    get => Locator.Instance.SignalLocator.GetSignal<ScoreChangedSignal>();
}
```

## Example using both services and signals

```csharp
using UnityEngine;
using Injector;

[Inject(typeof(IGameSettingsService), typeof(ScoreChangedSignal))]
public partial class GameplayPresenter : MonoBehaviour
{
    private void Start()
    {
        GameSettingsService.Save();
        ScoreChangedSignal.Subscribe(OnScoreChanged);
    }

    private void OnDestroy()
    {
        ScoreChangedSignal.Unsubscribe(OnScoreChanged);
    }

    private void OnScoreChanged(int value)
    {
        Debug.Log($"Received score: {value}");
    }
}
```

## Inspector usage

The project includes helper types to work with signals from the Inspector:

- `SignalListener`: listens to an `ISignal` without payload and invokes a `UnityEvent`
- `IntSignalListener`
- `FloatSignalListener`
- `StringSignalListener`
- `BoolSignalListener`
- `SignalDispatcherSO`: `ScriptableObject` used to dispatch signals
- `IntSignalDispatcherSO`
- `FloatSignalDispatcherSO`
- `StringSignalDispatcherSO`
- `BoolSignalDispatcherSO`

This makes it easier to connect events through the Inspector without writing code for every case.

## Commands

There is also a small command abstraction:

```csharp
public sealed class OpenMenuCommand : ICommand
{
    public void Execute()
    {
        // ...
    }
}
```

Or with payload:

```csharp
public sealed class UpdateScoreCommand : ICommand<int>
{
    public void Execute(int score)
    {
        // ...
    }
}
```

`ICommand` and `ICommand<TData>` already include default methods to bind and unbind signals:

```csharp
command.Bind<PlayerDiedSignal>();
command.Unbind<PlayerDiedSignal>();
```

## Expected package structure

Right now the repository is organized as a Unity project, but the package-relevant code mostly lives in:

- `Assets/Src/Runtime`
- `Assets/Src/Editor`
- `Assets/Analyzers/SourceGenerators.dll`

If you want to ship it as a UPM package, the README should stay with the package and the analyzer must remain available so code generation still works.

## Important notes about the current implementation

These details are useful if you want to use the package exactly as it works today:

- `InjectAttribute` can only be applied to classes.
- The generated injection does not assign serialized fields; it generates properties and proxies.
- Services are resolved lazily on first use.
- `ServiceLocator` depends on `Resources/ServiceContainerConfig.asset` being present.
- Signals need a parameterless constructor for generic locator usage.
- The generator identifies `services` through `IService` and `signals` through `ISignal`.

## Current limitations

- There is no automatic fallback if `ServiceContainerConfig` is missing; runtime only logs an error.
- Generated service property names depend on the type names passed to the attribute.
- Initial service loading uses reflection across assemblies to match the types stored in the asset with types available at runtime.
- The repository is not yet in a final UPM package layout with its own package-level `package.json`.

## Quick start summary

To use the package:

1. Create interfaces that inherit from `IService`.
2. Implement them and register the instances in `ServiceContainerConfig`.
3. Create signals inheriting from `Signal` or `Signal<T>`.
4. Mark `partial` classes with `[Inject(typeof(...))]`.
5. Use the generated members to access services and signals without manually resolving everything every time.
