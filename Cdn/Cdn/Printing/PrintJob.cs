using System;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Security;
using TomPIT.Serialization;

namespace TomPIT.Cdn.Printing
{
   internal class PrintJob : DispatcherJob<IPrintQueueMessage>
   {
      private TimeoutTask _timeout = null;
      public PrintJob(IDispatcher<IPrintQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
      {
      }

      protected override void DoWork(IPrintQueueMessage item)
      {
         _timeout = new TimeoutTask(() =>
         {
            MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Ping(item.PopReceipt);

            return Task.CompletedTask;
         }, TimeSpan.FromMinutes(4), Cancel);

         _timeout.Start();

         try
         {
            Invoke(item);
            MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Complete(item.PopReceipt);
         }
         finally
         {
            _timeout.Stop();
         }
      }

      private void Invoke(IPrintQueueMessage message)
      {
         try
         {
            EventLog.WriteInfo($"Handling printQueueMessage with id {message.Id} and token {message.Token}.");

            var provider = MiddlewareDescriptor.Current.Tenant.GetService<IDocumentService>().GetProvider(message.Provider);

            if (provider is null)
               throw new RuntimeException($"{SR.ErrPrintingProviderResolve} ({message.Provider})");

            if (Shell.GetService<IRuntimeService>().Platform == Platform.OnPrem)
               provider.Print(message);
            else
            {
               if (string.IsNullOrWhiteSpace(message.Arguments))
                  return;

               var args = Serializer.Deserialize<JObject>(message.Arguments);
               var printer = Serializer.Deserialize<Printer>(args.Required<string>("printer"));

               if (printer is null)
                  return;

               var report = provider.Create(message);

               if (report is not null)
               {
                 var spoolerToken = MiddlewareDescriptor.Current.Tenant.GetService<IPrintingSpoolerManagementService>().Insert(report.MimeType, printer.Name, Convert.ToBase64String(report.Content), message.SerialNumber, ResolveUserToken(message.User), message.CopyCount);

                  EventLog.WriteInfo($"PrintQueueMessage with id {message.Id} and token {message.Token} forwarded to spooler as {spoolerToken}.");
               }
               
            }
         }
         catch (Exception ex)
         {
            EventLog.WriteError($"Error handling printQueueMessage with id {message.Id} and token {message.Token}: {ex}");
            MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Write(new LogEntry
            {
               Category = "Cdn",
               Level = System.Diagnostics.TraceLevel.Error,
               Message = ex.Message,
               Source = nameof(PrintJob),
               EventId = MiddlewareEvents.Printing
            });

            throw;
         }
      }
      private Guid ResolveUserToken(string user)
      {
         if (string.IsNullOrWhiteSpace(user))
            return default;

         if (MiddlewareDescriptor.Current.Tenant.GetService<IUserService>().Select(user) is IUser userEntity)
            return userEntity.Token;

         return default;
      }

      protected override void OnError(IPrintQueueMessage item, Exception ex)
      {
         MiddlewareDescriptor.Current.Tenant.GetService<IPrintingManagementService>().Error(item.PopReceipt, ex.Message);
      }
   }
}
