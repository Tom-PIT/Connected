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
         if (!Shell.Configuration.RootElement.TryGetProperty("connectionStrings", out JsonElement element))
            return;

         if (!element.TryGetProperty("sys", out JsonElement sys))
            return;

         Sys = sys.GetString();
      }
   }
}
