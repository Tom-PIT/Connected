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

			DataModel.Queue.Enqueue(Queue, Serializer.Serialize(message),
				TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);
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
			return DataModel.Queue.Dequeue(count, TimeSpan.FromMinutes(5), QueueScope.System, Queue);
		}

		public void Ping(Guid popReceipt, TimeSpan nextVisible)
		{
			var m = DataModel.Queue.Select(popReceipt);

			if (m == null)
				return;

			DataModel.Queue.Ping(popReceipt, nextVisible);
		}

		public void Complete(Guid popReceipt)
		{
			var m = DataModel.Queue.Select(popReceipt);

			if (m == null)
				return;

			var e = Resolve(m);

			if (e != null)
				Delete(e.Identifier);

			DataModel.Queue.Complete(popReceipt);
		}

		private IDevelopmentError Resolve(IQueueMessage message)
		{
			var d = Serializer.Deserialize<JObject>(message.Message);

			var error = d.Required<Guid>("error");

			return Select(error);
		}
	}
}
