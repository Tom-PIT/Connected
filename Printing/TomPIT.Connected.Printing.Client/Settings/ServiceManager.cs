using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Connected.Printing.Client
{
    static class ServiceManager
    {
        internal static void Install(string username, string password) 
        {
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = "sc.exe",
                Arguments = $" create TomPIT.Connected.Printing.Client type= own start= auto binpath= \"dotnet.exe {System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Replace("file:///", "")}\" obj= {username} displayname= \"TomPIT.Connected.Printing.Client\" password= {password}"
            };

            var process = new Process 
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();
        }

        internal static void Uninstall() 
        {
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = "sc.exe",
                Arguments = $" delete TomPIT.Connected.Printing.Client"
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();
        }

    }
}
