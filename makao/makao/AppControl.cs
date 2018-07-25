using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace Makao
{
    public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

    class AppControl : IDisposable
    {
        private static AppControl ctrlInstance = null;
        protected static MainForm mainWindow;
        private static Font appFont = new Font(FontFamily.GenericMonospace, 12);

        public static MainForm MainWindow
        {
            get
            {
                return mainWindow;
            }
            set
            {
                mainWindow = value;
            }
        }

        public static AppControl CtrlInstance
        {
            get
            {
                return ctrlInstance;
            }
            set
            {
                if (ctrlInstance != null)
                    ctrlInstance.Dispose();
                ctrlInstance = value;
                if (ctrlInstance != null)
                    ctrlInstance.InitializeWindow();

                mainWindow.Invalidate();
            }
        }

        public static Font AppFont
        {
            get
            {
                return appFont;
            }
        }

        public virtual void Dispose()
        {
        }
        public virtual void InitializeWindow()
        {
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(
            int idHook,
            [MarshalAs(UnmanagedType.FunctionPtr)]HookProc lpfn,
            IntPtr hMod,
            int threadId
            );

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int code,
            IntPtr wParam,
            IntPtr lParam
            );

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
    }
}
