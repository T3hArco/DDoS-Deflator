using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Principal;
using System.Reflection;
using System.IO;

namespace DDoSMitigator
{
    static class Program
    {
        public static string path;
        [STAThread]
        static void Main()
        {
            if (!_isAdministrator())
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Assembly.GetExecutingAssembly().Location;
                psi.Verb = "runas";
            again:
                try
                {
                    Process.Start(psi);
                }
                catch { goto again; }
            }
            else
            {
                path = Environment.GetEnvironmentVariable("APPDATA") + "\\" + "DDoSMitigator";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    File.WriteAllBytes(path + "\\" + "cports.exe", Properties.Resources.cports);
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }

        private static bool _isAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
