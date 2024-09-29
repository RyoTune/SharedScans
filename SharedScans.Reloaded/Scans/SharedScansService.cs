using Reloaded.Hooks.Definitions;
using SharedScans.Interfaces;

namespace SharedScans.Reloaded.Scans;

internal class SharedScansService : ISharedScans
{
    private readonly ScansManager scansManager;
    private readonly IReloadedHooks hooks;
    private readonly List<IScanListener> listeners = [];

    public SharedScansService(ScansManager scansManager, IReloadedHooks hooks)
    {
        this.scansManager = scansManager;
        this.hooks = hooks;
    }

    public void AddScan(string id, string? pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            Log.Verbose($"{id}: No pattern given.");
            return;
        }

        this.scansManager.Add(id, pattern, (hooks, result) => this.SendResult(id, hooks, result));
    }

    public void AddScan<TFunction>(string? pattern)
        => this.AddScan(typeof(TFunction).Name, pattern);

    public void CreateListener(string id, Action<nint> success)
    {
        var listener = new ResultListener(id, success);
        this.listeners.Add(listener);
    }

    public void CreateListener<TFunction>(Action<nint> success)
        => this.CreateListener(typeof(TFunction).Name, success);

    public HookContainer<TFunction> CreateHook<TFunction>(TFunction hookFunction, string owner)
    {
        var id = typeof(TFunction).Name;
        var listener = new HookListener<TFunction>(owner, id, hookFunction);
        this.listeners.Add(listener);
        return listener.Hook;
    }

    public WrapperContainer<TFunction> CreateWrapper<TFunction>(string owner)
    {
        var id = typeof(TFunction).Name;
        var listener = new WrapperListener<TFunction>(owner, id);
        this.listeners.Add(listener);
        return listener.Wrapper;
    }

    public void Broadcast<TFunction>(nint result) => this.Broadcast(typeof(TFunction).Name, result);

    public void Broadcast(string id, nint result) => this.SendResult(id, this.hooks, result);

    private void SendResult(string id, IReloadedHooks hooks, nint result)
    {
        var scanListeners = this.listeners.Where(x => x.Id == id).ToArray();
        foreach (var listener in scanListeners)
        {
            listener.SetScanFound(hooks, result);
        }

        if (scanListeners.Length > 0)
        {
            Log.Information($"Scan for \"{id}\" given to {scanListeners.Length} listener(s).");
        }
    }
}
