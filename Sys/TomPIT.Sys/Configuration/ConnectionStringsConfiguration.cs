using Microsoft.Extensions.Configuration;

using System.Text.Json;

namespace TomPIT.Sys.Configuration
{
   internal static class ConnectionStringsConfiguration
   {
      static ConnectionStringsConfiguration()
      {
         Initialize();
      }

      public static string Sys { get; set; }

      private static void Initialize()
      {
         Sys = Shell.Configuration.GetValue<string>("connectionStrings:sys");
      }
   }
}
