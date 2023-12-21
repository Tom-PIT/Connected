using System;
using System.Configuration;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using TomPIT.Connectivity;
using TomPIT.Distributed;
using TomPIT.Environment;
using TomPIT.Middleware;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Services
{
   internal class Preloader : HostedService
   {
      private HttpConnection _connection = null;
      private Uri _sysUri = null;

      public Preloader()
      {
         IntervalTimeout = TimeSpan.FromSeconds(45);
      }
      protected override bool OnInitialize(CancellationToken cancel)
      {
         return DataModel.Initialized;
      }

      protected override Task OnExecute(CancellationToken cancel)
      {
         try
         {
            var ds = DataModel.InstanceEndpoints.Query();

            foreach (var i in ds)
               Load(i);
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }

         return Task.CompletedTask;
      }

      private void Load(IInstanceEndpoint endpoint)
      {
         if (string.IsNullOrWhiteSpace(endpoint.Url) || endpoint.Status == InstanceStatus.Disabled)
            return;

         try
         {
            Connection.Get<string>($"{endpoint.Url}/sys/ping");
         }
         catch { }
      }

      private HttpConnection Connection => _connection ??= new();
   }
}
