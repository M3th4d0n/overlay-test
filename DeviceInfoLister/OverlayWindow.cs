using System.Runtime.InteropServices;
using OverlayApp.WinApi;

namespace OverlayApp;

public class OverlayWindow {
    private const int BUTTON1_ID = 101;
    private const int BUTTON2_ID = 102;
    
    private const int COLOR_WINDOW = 1; // transparency overlay backunderground yopta 
    private IntPtr hInstance;
    private IntPtr hWnd;

    private bool overlayVisible = true;


    private NativeMethods.WndProc wndProcDelegate;

    public void Run() {
        var className = "pizda";
        hInstance = NativeMethods.GetModuleHandle(null);
        
        wndProcDelegate = WndProc;


        var wndClassEx = new NativeMethods.WNDCLASSEX {
            cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.WNDCLASSEX)),
            style = 0,
            lpfnWndProc = wndProcDelegate,
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = hInstance,
            hIcon = IntPtr.Zero,
            hCursor = IntPtr.Zero,

            hbrBackground = 1 + COLOR_WINDOW,
            lpszMenuName = null,
            lpszClassName = className,
            hIconSm = IntPtr.Zero
        };

        var regResult = NativeMethods.RegisterClassEx(ref wndClassEx);
        if (regResult == 0) {
            Console.WriteLine("Не удалось зарегистрировать класс окна.");
            return;
        }


        var screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        var screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);


        var extendedStyle = NativeMethods.WS_EX_TOPMOST;
        hWnd = NativeMethods.CreateWindowEx(
            extendedStyle,
            className,
            "Interactive Overlay",
            NativeMethods.WS_POPUP | NativeMethods.WS_VISIBLE,
            0, 0,
            screenWidth, screenHeight,
            IntPtr.Zero,
            IntPtr.Zero,
            hInstance,
            IntPtr.Zero);

        if (hWnd == IntPtr.Zero) {
            Console.WriteLine("Не удалось создать окно.");
            return;
        }


        ExtendGlassFrame();


        CreateChildControls();

        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_SHOW);
        NativeMethods.UpdateWindow(hWnd);


        if (!NativeMethods.RegisterHotKey(IntPtr.Zero, NativeMethods.HOTKEY_ID,
                NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT, NativeMethods.VK_L))
            Console.WriteLine("Не удалось зарегистрировать горячую клавишу.");


        NativeMethods.MSG msg;
        while (NativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0)) {
            NativeMethods.TranslateMessage(ref msg);

            if (msg.message == NativeMethods.WM_HOTKEY && (uint)msg.wParam == NativeMethods.HOTKEY_ID)
                ToggleOverlay();
            else
                NativeMethods.DispatchMessage(ref msg);
        }
    }

    private void ToggleOverlay() {
        if (overlayVisible) {
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_HIDE);
            overlayVisible = false;
        }
        else {
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_SHOW);
            overlayVisible = true;
        }
    }

    private void ExtendGlassFrame() {
        var margins = new NativeMethods.MARGINS {
            cxLeftWidth = -1,
            cxRightWidth = -1,
            cyTopHeight = -1,
            cyBottomHeight = -1
        };

        var result = NativeMethods.DwmExtendFrameIntoClientArea(hWnd, ref margins);
        if (result != 0) Console.WriteLine("DwmExtendFrameIntoClientArea вернул ошибку: " + result);
    }

    private void CreateChildControls() {

        var hButton1 = NativeMethods.CreateWindowEx(
            0,
            "BUTTON",
            "Button 1",
            NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE,
            50, 50, 100, 30,
            hWnd,
            new IntPtr(BUTTON1_ID),
            hInstance,
            IntPtr.Zero);

        var hButton2 = NativeMethods.CreateWindowEx(
            0,
            "BUTTON",
            "Button 2",
            NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE,
            50, 100, 100, 30,
            hWnd,
            new IntPtr(BUTTON2_ID),
            hInstance,
            IntPtr.Zero);
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) {
        switch (msg) {
            case NativeMethods.WM_PAINT: {
                NativeMethods.PAINTSTRUCT ps;
                var hdc = NativeMethods.BeginPaint(hWnd, out ps);
                NativeMethods.TextOut(hdc, 200, 50, "Interactive Overlay Active", "Interactive Overlay Active".Length);
                NativeMethods.EndPaint(hWnd, ref ps);
            }
                break;

            case NativeMethods.WM_COMMAND: {

                var controlId = (int)((uint)wParam & 0xFFFF);
                if (controlId == BUTTON1_ID)
                    NativeMethods.MessageBox(hWnd, "Button 1 clicked!", "Notification", 0);
                else if (controlId == BUTTON2_ID)
                    NativeMethods.MessageBox(hWnd, "Button 2 clicked!", "Notification", 0);
            }
                break;

            case NativeMethods.WM_DESTROY:

                NativeMethods.UnregisterHotKey(IntPtr.Zero, NativeMethods.HOTKEY_ID);
                NativeMethods.PostQuitMessage(0);
                break;

            default:
                return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        return IntPtr.Zero;
    }
}