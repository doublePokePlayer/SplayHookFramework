using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X64;
using SharpGen.Runtime;
using SplayHookFramework.Misc;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using static SplayHookFramework.Misc.Native;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;

namespace SplayHookFramework.Hooking.ImGui;

public class ImGuiDx11Hook() : ImGuiHookBase(nameof(ImGuiDx11Hook))
{
    /// <summary>
    /// Present函数在SwapChain虚表的索引下标
    /// </summary>
    private const int PresentIndex = 8;

    /// <summary>
    /// ResizeBuffers函数在SwapChain虚表的索引下标
    /// </summary>
    private const int ResizeBuffersIndex = 13;

    /// <summary>
    /// DX11 DXGI SwapChain 虚表.
    /// </summary>
    public static IVirtualFunctionTable? SwapChainVTable { get; private set; }

    [Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(CallingConventions.Stdcall)]
    public struct Present
    {
        public FuncPtr<IntPtr, uint, uint, IntPtr> Value;
    }

    [Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
    [Reloaded.Hooks.Definitions.X86.Function(CallingConventions.Stdcall)]
    public struct ResizeBuffers
    {
        public FuncPtr<IntPtr, uint, uint, uint, Format, uint, IntPtr> Value;
    }

    private ID3D11Device? _device;
    private IDXGISwapChain _swapChain = null!;
    private ID3D11DeviceContext? _deviceContext;
    private ID3D11RenderTargetView _renderTargetView = null!;

    private IHook<Present> _presentHook = null!;
    private IHook<ResizeBuffers> _resizeBuffersHook = null!;

    public override unsafe void Initial()
    {
        try
        {
            if (!IsApiSupported())
            {
                Debug.WriteLine("当前进程不是Dx11渲染");
                return;
            }

            FeatureLevel[] features = [FeatureLevel.Level_10_0, FeatureLevel.Level_11_0];
            var swapChainDesc = new SwapChainDescription
            {
                BufferCount = 1,
                BufferDescription = new ModeDescription
                {
                    Format = Format.R8G8B8A8_UNorm
                },
                BufferUsage = Usage.RenderTargetOutput,
                OutputWindow = GetForegroundWindow(),
                SampleDescription = new SampleDescription(1, 0),
                Windowed = true,
                SwapEffect = SwapEffect.Discard
            };

            var res = D3D11.D3D11CreateDeviceAndSwapChain(
                null,
                DriverType.Hardware,
                DeviceCreationFlags.None,
                features,
                swapChainDesc,
                out var swapChain,
                out var device,
                out _,
                out _);

            if (res == Result.Ok && swapChain != null)
            {
                SwapChainVTable = ISplayHook.InternalHooks.VirtualFunctionTableFromObject(swapChain.NativePointer, 18);

                Debug.DebugWriteLine($"Present在虚表地址: 0X{SwapChainVTable[PresentIndex].EntryAddress:X}");
                Debug.DebugWriteLine($"ResizeBuffers在虚表地址: 0X{SwapChainVTable[ResizeBuffersIndex].EntryAddress:X}");

                swapChain.Release();
                device?.Release();

                var presentPtr = SwapChainVTable[8].FunctionPointer;
                var resizeBufferPtr = SwapChainVTable[13].FunctionPointer;

                _presentHook = ISplayHook.InternalHooks.CreateHook<Present>(
                    (delegate* unmanaged[Stdcall]<IntPtr, uint, uint, IntPtr>)&PresentImplStatic,
                    presentPtr
                ).Activate();

                _resizeBuffersHook = ISplayHook.InternalHooks.CreateHook<ResizeBuffers>(
                    (delegate* unmanaged[Stdcall]<IntPtr, uint, uint, uint, Format, uint, IntPtr>)
                    &ResizeBuffersImplStatic,
                    resizeBufferPtr
                ).Activate();
            }
            else
            {
                Debug.WriteError("无法成功创建swapChain, 寄寄寄");
            }
        }
        catch (Exception e)
        {
            Debug.DebugWriteLine(e.ToString());
        }
    }

    public override void Release()
    {
        DisableAll();
        SetWindowLongPtr(WindowHandle, (int)GWL.GWL_WNDPROC, WndProcHook.OriginalWndProc);
        if (Inited)
        {
            ImGuiNET.ImGui.ImGui_ImplDX11_Shutdown();
            ImGuiNET.ImGui.ImGui_ImplWin32_Shutdown();
            ImGuiNET.ImGui.DestroyContext();

            _deviceContext?.Release();
            _device?.Release();
        }

        if (GlobalSettings.UseConsole) FreeConsole();

        FreeLibrary(GlobalSettings.DllInstance);
    }

    private delegate bool Free(nint hModule);

    public override void Enable()
    {
        _presentHook.Enable();
        _resizeBuffersHook.Enable();
    }

    public override void Disable()
    {
        _presentHook.Disable();
        _resizeBuffersHook.Disable();
    }

    public override void Dispose()
    {
        Release();
    }

    /// <summary>
    /// 支持的dll库
    /// </summary>
    private static readonly string[] SupportedDlls =
    [
        "d3d11.dll",
        "d3d11_1.dll",
        "d3d11_2.dll",
        "d3d11_3.dll",
        "d3d11_4.dll"
    ];

    protected override bool IsApiSupported()
    {
        return SupportedDlls.Any(dll => GetModuleHandle(dll) != IntPtr.Zero);
    }

    protected override void PreRender()
    {
        ImGuiNET.ImGui.ImGui_ImplWin32_NewFrame();
        ImGuiNET.ImGui.ImGui_ImplDX11_NewFrame();
        ImGuiNET.ImGui.NewFrame();
    }

    protected unsafe IntPtr Render(IntPtr swapChainPtr, uint syncInterval, uint flags)
    {
        try
        {
            if (!Inited)
            {
                _swapChain = ComObject.As<IDXGISwapChain>(swapChainPtr);
                _swapChain.GetDevice(out IDXGIDevice? device);
                if (device != null)
                {
                    _device = device.QueryInterface<ID3D11Device>();
                    if (_device != null)
                    {
                        _deviceContext = _device.ImmediateContext;
                        var description = _swapChain.Description;
                        WindowHandle = description.OutputWindow;

                        _swapChain.GetBuffer<ID3D11Texture2D>(0, out var buf);

                        if (buf != null)
                        {
                            _renderTargetView = _device.CreateRenderTargetView(buf);
                            buf.Release();

                            WndProcHook.OriginalWndProc = SetWindowLongPtr(WindowHandle, (int)GWL.GWL_WNDPROC,
                                Marshal.GetFunctionPointerForDelegate(NewWndProc));
                        }
                        else
                        {
                            Debug.WriteError("GetBuffer失败");
                        }

                        ImGuiNET.ImGui.CreateContext(null);
                        ImGuiNET.ImGui.ImGui_ImplWin32_Init(WindowHandle);
                        ImGuiNET.ImGui.ImGui_ImplDX11_Init(_device.NativePointer, _deviceContext.NativePointer);
                    }
                    else
                    {
                        Debug.WriteError("获取ID3D11Device失败");
                    }
                }
                else
                {
                    Debug.WriteError("获取device失败");
                }

                Inited = true;
            }


            if (Open)
            {
                PreRender();
                ImGuiNET.ImGui.ShowDemoWindow();
                PostRender();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return _presentHook.OriginalFunction.Value.InvokeAsStdcall(swapChainPtr, syncInterval, flags);
    }

    #region Hook函数静态实现

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static IntPtr PresentImplStatic(IntPtr swapChainPtr, uint syncInterval, uint flags)
    {
        return ISplayHook.HooksDictionary[nameof(ImGuiDx11Hook)]
            .As<ImGuiDx11Hook>()
            .Render(swapChainPtr, syncInterval, flags);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static IntPtr ResizeBuffersImplStatic(IntPtr swapChainPtr, uint bufferCount, uint width, uint height,
        Format newFormat, uint swapChainFlags)
    {
        return ISplayHook.HooksDictionary[nameof(ImGuiDx11Hook)].As<ImGuiDx11Hook>()
            .Resize(swapChainPtr, bufferCount, width, height, newFormat, swapChainFlags);
    }

    #endregion

    protected override void PostRender()
    {
        ImGuiNET.ImGui.EndFrame();
        ImGuiNET.ImGui.Render();
        _deviceContext?.OMSetRenderTargets(1, [_renderTargetView]);
        ImGuiNET.ImGui.ImGui_ImplDX11_RenderDrawData(ImGuiNET.ImGui.GetDrawData());
    }

    protected override void PreResize()
    {
        // 恢复窗口过程
        SetWindowLongPtr(WindowHandle, (int)GWL.GWL_WNDPROC, WndProcHook.OriginalWndProc);
        if (Inited)
        {
            Inited = false;
            ImGuiNET.ImGui.ImGui_ImplDX11_Shutdown();
            ImGuiNET.ImGui.ImGui_ImplWin32_Shutdown();
            ImGuiNET.ImGui.DestroyContext();

            _device?.Release();
            _deviceContext?.Release();
            _renderTargetView?.Release();
        }
    }

    /// <summary>
    /// Resize函数Hook实现
    /// </summary>
    /// <param name="swapChain"></param>
    /// <param name="bufferCount"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="newFormat"></param>
    /// <param name="swapChainFlags"></param>
    /// <returns></returns>
    protected unsafe IntPtr Resize(IntPtr swapChain, uint bufferCount, uint width, uint height, Format newFormat,
        uint swapChainFlags)
    {
        PreResize();

        var res = _resizeBuffersHook.OriginalFunction.Value.Invoke(swapChain, bufferCount, width, height, newFormat,
            swapChainFlags);

        PostResize();

        return res;
    }

    protected override void PostResize()
    {
    }
}