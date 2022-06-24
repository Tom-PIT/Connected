using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Connected.Printing.Client
{
    [Verb("install", isDefault: false, HelpText = "Install service")]
    public class InstallOptions 
    {
        [Option(shortName: 'u', longName:"username", Default = "", HelpText = "The name of the user under which to execute", Required = true)]
        public string Username { get; set; }

        [Option(shortName: 'p', longName: "password", Default = "", HelpText = "The password for the entered user", Required = true)]
        public string Password { get; set; }
    }

    [Verb("uninstall", isDefault: false, HelpText = "Uninstall service")]
    public class UninstallOptions
    {

    }

    [Verb("run", isDefault: true , HelpText = "Run service")]
    public class RunOptions
    {

    }
}
