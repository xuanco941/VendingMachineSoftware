using MyApplication;
using System;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VendingMachine
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

        }

        private void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView21.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webView21.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webView21.Source = new Uri("https://automaticpayment.somee.com/");

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

        }
    }
}
