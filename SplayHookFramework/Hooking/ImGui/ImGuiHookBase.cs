using static SplayHookFramework.Hooking.ImGui.WndProcHook;

namespace SplayHookFramework.Hooking.ImGui;

/// <summary>
/// Hook渲染引擎 绘制ImGui抽象类
/// </summary>
public abstract class ImGuiHookBase(string name) : SplayHook(name)
{
    /// <summary>
    /// 判断当前进程是否支持此渲染方式.
    /// </summary>
    protected abstract bool IsApiSupported();

    /// <summary>
    /// 窗口句柄
    /// </summary>
    public static IntPtr WindowHandle { get; set; }

    protected static WndProcDelegate NewWndProc = WndProcImpl;

    /// <summary>
    /// 第一次调用要初始化ImGui相关
    /// Resize后也需要重新初始化
    /// </summary>
    public static bool Inited { get; protected set; }

    public static bool Open = true;

    /// <summary>
    /// 取消Hook 释放资源
    /// 使用了ImGui, 那就应该在不用ImGui的时候释放你的功能和画面
    /// </summary>
    public abstract void Release();

    /// <summary>
    /// 渲染之前
    /// </summary>
    protected abstract void PreRender();

    /// <summary>
    /// 渲染之后
    /// </summary>
    protected abstract void PostRender();

    /// <summary>
    /// Resize之前
    /// </summary>
    protected abstract void PreResize();

    /// <summary>
    /// Resize之后
    /// </summary>
    protected abstract void PostResize();
}