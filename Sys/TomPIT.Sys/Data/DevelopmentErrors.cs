using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	internal class DevelopmentErrors
	{
		private const string Queue = "devautofix";
		public void AutoFix(string provider, Guid error)
		{
			var message = new JObject
			{
				{"provider", provider },
				{"error", error }
			};

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(Queue, Serializer.Serialize(message),
				TimeSpan.FromDays(2), TimeSpan.Zero, SysDb.Messaging.QueueScope.System);
		}
		public List<IDevelopmentComponentError> Query(Guid microService, ErrorCategory category)
		{
			IMicroService ms = null;

			if (microService != Guid.Empty)
			{
				ms = DataModel.MicroServices.Select(microService);

				if (ms == null)
					throw new SysException(SR.ErrMicroServiceNotFound);
			}

			return Shell.GetService<IDatabaseService>().Proxy.Development.Errors.Query(ms, category);
		}

		public IDevelopmentComponentError Select(Guid identifier)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Development.Errors.Select(identifier);
		}
		public void Delete(Guid identifier)
		{
			Shell.GetService<IDatabaseService>().Proxy.Development.Errors.Delete(identifier);
		}


		public void Clear(Guid component, Guid element, ErrorCategory category)
		{
			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.Errors.Clear(c, element, category);
		}

		public void Insert(Guid microService, Guid component, List<IDevelopmentError> errors)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.Errors.Insert(ms, c, errors);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.DequeueSystem(Queue, count, TimeSpan.FromMinutes(5));
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			var e = Resolve(m);

			if (e != null)
				Delete(e.Identifier);

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(popReceipt);
		}

		private IDevelopmentError Resolve(IQueueMessage message)
		{
			var d = Serializer.Deserialize<JObject>(message.Message);

			var error = d.Required<Guid>("error");

			return Select(error);
		}
	}
}
