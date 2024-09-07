# SharedScans

A Reloaded II mod for easily sharing sigscans and hooks between mods.

# Usage
1. In *Visual Studio*, install the `SharedScans.Interfaces` nuget package.
2. Add `SharedScans.Reloaded` to your mod's **Mod Dependencies** in `ModConfig.json`.
3. Get the `ISharedScans` API with: `modLoader.GetController<ISharedScans>().TryGetTarget(out var scans)`

# API

## Adding Scans
Adding scans requires two things: an ID and a pattern.

### `AddScan<TFunction>(string? pattern)`

The preferred method for adding a scan, where `TFunction` is the function delegate type of the pattern. The ID of the scan will be `TFunction`, which simplifies hooking it later.

### `AddScan(string id, string? pattern)`

Alternatively, you can manually set the ID of the scan.

## Function Hooks and Wrappers
Listen for and then create a hook or wrapper with the given scan.

### `HookContainer<TFunction> CreateHook<TFunction>(TFunction hookFunction, string owner)`
Creates a function hook, using `TFunction` as the scan ID to listen for.

### `WrapperContainer<TFunction> CreateWrapper<TFunction>(string owner)`
Creates a function wrapper, using `TFunction` as the scan ID to listen for.

## Listeners
Listens for the given scan and gets the pattern result.

### `CreateListener(string id, Action<nint> success)`

### `CreateListener<TFunction>(Action<nint> success)`
