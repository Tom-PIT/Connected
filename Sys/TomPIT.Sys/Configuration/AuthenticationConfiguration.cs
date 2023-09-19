namespace TomPIT.Sys.Configuration
{
   internal static class AuthenticationConfiguration
   {
      static AuthenticationConfiguration()
      {
         JwToken = new();
      }

      public static JwTokenConfiguration JwToken { get; private set; }
   }
}
