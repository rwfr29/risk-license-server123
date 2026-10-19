using System;
using System.Threading;
using System.Windows.Forms;

namespace RISKHackTool
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show(
                    "UI thread exception:\n\n" + e.Exception,
                    "Unhandled Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show(
                    "Fatal error:\n\n" + (ex?.ToString() ?? "unknown"),
                    "Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RISKHack());
        }
    }
}