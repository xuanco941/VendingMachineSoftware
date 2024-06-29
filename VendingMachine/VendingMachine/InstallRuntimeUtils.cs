using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyApplication
{
    public class InstallRuntimeUtils
    {
        public static bool IsResetByWebView { get; set; } = false;

        public static void InstallWebView2()
        {
            if (!IsWebView2Installed())
            {
                try
                {
                    var setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MicrosoftEdgeWebview2Setup.exe");

                    Process process = new Process();
                    process.StartInfo.FileName = setupPath;
                    process.StartInfo.Arguments = "/silent /install";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    //File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(),"log_runtime.txt"), "\nĐang cài WebView2");

                    process.WaitForExit();
                    IsResetByWebView = true;

                }
                catch (Exception ex)
                {
                    //File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), "log_runtime.txt"), "\nWebview2: "+ex.Message);
                }
            }
            else
            {
                //File.AppendAllText(Path.Combine(Directory.GetCurrentDirectory(), "log_runtime.txt"), "\nWebview2: installed");
            }
        }

        public static bool IsWebView2Installed()
        {
            try
            {
                string ver = CoreWebView2Environment.GetAvailableBrowserVersionString();
                if (string.IsNullOrEmpty(ver))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            //var key1 = Registry.LocalMachine.OpenSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            //var key2 = Registry.LocalMachine.OpenSubKey(@"HKEY_CURRENT_USER\Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            //var key3 = Registry.LocalMachine.OpenSubKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            //var key4 = Registry.LocalMachine.OpenSubKey(@"HKEY_CURRENT_USER\Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");

            //if (key1 != null)
            //{
            //    return key.GetSubKeyNames().Any(name => name.Contains("WebView2"));
            //}
            //return false;
        }
    }
}
