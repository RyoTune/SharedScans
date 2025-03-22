using Reloaded.Hooks.Definitions;
using SharedScans.Interfaces;

namespace SharedScans.Reloaded.Scans;

internal class SharedScansService(IReloadedHooks hooks) : ISharedScans
{
    private readonly List<IScanListener> _listeners = [];

    public void AddScan(string id, string? pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            Log.Verbose($"{id}: No pattern given.");
            return;
        }

        ScanHooks.Add(id, pattern, (hooks, result) => SendResult(id, hooks, result));
    }

    public void AddScan<TFunction>(string? pattern)
        => AddScan(typeof(TFunction).Name, pattern);

    public void CreateListener(string id, Action<nint> success)
    {
        var listener = new ResultListener(id, success);
        _listeners.Add(listener);
    }

    public void CreateListener<TFunction>(Action<nint> success)
        => CreateListener(typeof(TFunction).Name, success);

    public HookContainer<TFunction> CreateHook<TFunction>(TFunction hookFunction, string owner)
    {
        var id = typeof(TFunction).Name;
        var listener = new HookListener<TFunction>(owner, id, hookFunction);
        _listeners.Add(listener);
        return listener.Hook;
    }

    public WrapperContainer<TFunction> CreateWrapper<TFunction>(string owner)
    {
        var id = typeof(TFunction).Name;
        var listener = new WrapperListener<TFunction>(owner, id);
        _listeners.Add(listener);
        return listener.Wrapper;
    }

    public void Broadcast<TFunction>(nint result) => Broadcast(typeof(TFunction).Name, result);

    public void Broadcast(string id, nint result) => SendResult(id, hooks, result);

    private void SendResult(string id, IReloadedHooks hooks, nint result)
    {
        var scanListeners = _listeners.Where(x => x.Id == id).ToArray();
        foreach (var listener in scanListeners)
        {
            listener.SetScanFound(hooks, result);
        }

        if (scanListeners.Length > 0)
        {
            Log.Debug($"Scan for \"{id}\" given to {scanListeners.Length} listener(s).");
        }
    }
}
