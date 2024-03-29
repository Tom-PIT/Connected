﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using TomPIT.Cdn;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Model.Printing
{
   public class PrintingSpoolerModel
   {
      private const string Queue = "printingSpooler";
      public Guid Insert(string mime, string printer, string content, long serialNumber, Guid identity, int copyCount = 1)
      {
         var token = Guid.NewGuid();

         var message = new JObject
            {
                { "id",token},
                { "printer", printer},
                { "serialNumber", serialNumber},
                { "identity", identity},
                { "copyCount", copyCount}
            };

         Shell.GetService<IDatabaseService>().Proxy.Printing.InsertSpooler(token, DateTime.UtcNow, mime, printer, content, identity, copyCount);
         DataModel.Queue.Enqueue(Queue, Serializer.Serialize(message), null, TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

         return token;
      }

      public ImmutableList<IQueueMessage> Dequeue(int count)
      {
         return DataModel.Queue.Dequeue(count, TimeSpan.FromMinutes(1), QueueScope.System, Queue);
      }

      public void Ping(Guid popReceipt, TimeSpan nextVisible)
      {
         DataModel.Queue.Ping(popReceipt, nextVisible);
      }

      public void Complete(Guid popReceipt)
      {
         var m = DataModel.Queue.Select(popReceipt);

         if (m is null)
            return;

         DataModel.Queue.Complete(popReceipt);
         var job = ResolveJob(m);

         if (job is not null)
            Delete(job.Token);
      }

      public void Delete(Guid token)
      {
         var job = Select(token);

         if (job is null)
            return;

         Shell.GetService<IDatabaseService>().Proxy.Printing.DeleteSpooler(job);
      }

      public IPrintSpoolerJob Select(Guid token)
      {
         return Shell.GetService<IDatabaseService>().Proxy.Printing.SelectSpooler(token);
      }

      private IPrintSpoolerJob ResolveJob(IQueueMessage message)
      {
         var d = Serializer.Deserialize<JObject>(message.Message);

         var id = d.Required<Guid>("id");

         return Select(id);
      }

      public async Task Flush()
      {
         await Task.CompletedTask;
      }
   }
}
