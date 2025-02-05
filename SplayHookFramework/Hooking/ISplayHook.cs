using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;

namespace SplayHookFramework.Hooking;

public interface ISplayHook : IDisposable
{
    /// <summary>
    /// 存储所有的Hook实例
    /// </summary>
    public static Dictionary<string, ISplayHook> HooksDictionary { get; set; } = [];

    protected static IReloadedHooks InternalHooks { get; set; } = new ReloadedHooks();

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; protected set; }

    /// <summary>
    /// 初始化Hook需要的变量
    /// </summary>
    void Initial();

    /// <summary>
    /// 启用Hook
    /// </summary>
    /// <returns></returns>
    void Enable();

    /// <summary>
    /// 禁用Hook
    /// </summary>
    /// <returns></returns>
    void Disable();

    /// <summary>
    /// 启用全部
    /// </summary>
    /// <returns></returns>
    void EnableAll();

    /// <summary>
    /// 禁用全部
    /// </summary>
    /// <returns></returns>
    void DisableAll();
}