using System;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        Gekka.Windows.USB.Input.InputTool tool;
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                tool = new Gekka.Windows.USB.Input.InputTool(hwnd);
                this.DataContext = tool;

                var h = System.Windows.Interop.HwndSource.FromHwnd(hwnd);
                h.AddHook(this.HwndSourceHook);

            };
        }


        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = tool.OnWindProc(msg, wParam, lParam);
            return IntPtr.Zero;
        }
    }
}
