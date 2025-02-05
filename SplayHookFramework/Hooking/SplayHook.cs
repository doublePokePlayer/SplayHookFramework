namespace SplayHookFramework.Hooking;

/// <summary>
/// Hook的抽象类 所有Hook都应该继承自它
/// </summary>
public abstract class SplayHook : ISplayHook
{
    public bool Enabled { get; set; }

    public abstract void Initial();

    public abstract void Enable();

    public abstract void Disable();

    protected SplayHook(string name)
    {
        ISplayHook.HooksDictionary.TryAdd(name, this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public abstract void Dispose();

    public void EnableAll()
    {
        foreach (var hook in ISplayHook.HooksDictionary) hook.Value.Enable();
    }

    public void DisableAll()
    {
        foreach (var hook in ISplayHook.HooksDictionary) hook.Value.Disable();
    }
}