using MyApplication;

namespace VendingMachine
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            Task.Run(() =>
            {
                InstallRuntimeUtils.InstallWebView2(); // cai webview2 runtime
                if (InstallRuntimeUtils.IsResetByWebView)
                {
                    var dialog = MessageBox.Show("Ứng dụng của bạn vừa cài đặt thêm các gói môi trường, bạn cần khởi động lại.", "Hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (dialog == DialogResult.OK)
                    {
                        // Chuyển về UI thread để gọi Application.Exit()
                        MethodInvoker invoker = () => Application.Exit();
                        if (Application.OpenForms[0].InvokeRequired)
                        {
                            Application.OpenForms[0].Invoke(invoker);
                        }
                        else
                        {
                            invoker();
                        }
                    }
                }


            });


            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}