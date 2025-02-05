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
    public IntPtr WindowHandle { get; set; }

    protected static WndProcDelegate NewWndProc = WndProcImpl;

    /// <summary>
    /// 第一次调用要初始化ImGui相关
    /// Resize后也要重新初始化
    /// </summary>
    public bool Inited { get; protected set; }

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
    /// Resize时执行
    /// </summary>
    protected abstract void Resize();

    /// <summary>
    /// Resize之后
    /// </summary>
    protected abstract void PostResize();
}