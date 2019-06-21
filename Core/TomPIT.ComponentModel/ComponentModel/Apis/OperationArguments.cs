using Newtonsoft.Json.Linq;
using System;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Events;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.ComponentModel.Apis
{
	[ExceptionSourceProperty(nameof(ExceptionSource))]
	public abstract class OperationArguments : DataModelContext, IApiExecutionScope
	{
		internal const string EventProvider = "TomPIT.Design.CodeAnalysis.Providers.EventProvider, TomPIT.Design";

		public OperationArguments(IExecutionContext sender, IApiOperation operation, JObject arguments) : base(sender)
		{
			Arguments = arguments ?? new JObject();
			Operation = operation;
		}

		public JObject Arguments { get; }
		protected IApiOperation Operation { get; }

		public IApi Api
		{
			get { return Operation.Closest<IApi>(); }
		}

		protected override string ExceptionSource
		{
			get
			{
				return Api == null || Operation == null
					? GetType().ShortName()
					: string.Format("{0}/{1}", Api.ComponentName(this), Operation.Name);
			}
		}

		[Obsolete("Call Services.Cdn.Event instead")]
		public Guid Event<T>([CodeAnalysisProvider(EventProvider)]string name, T e)
		{
			return Connection.GetService<IEventService>().Trigger(MicroService.Token, name, null, e);
		}
		[Obsolete("Call Services.Cdn.Event instead")]
		public Guid Event([CodeAnalysisProvider(EventProvider)]string name)
		{
			return Event(name, null);
		}
		[Obsolete("Call Services.Cdn.Event instead")]
		public Guid Event<T>([CodeAnalysisProvider(EventProvider)]string name, T e, IEventCallback callback)
		{
			return Connection.GetService<IEventService>().Trigger(MicroService.Token, name, callback, e);
		}
		[Obsolete("Call Services.Cdn.Event instead")]
		public Guid Event([CodeAnalysisProvider(EventProvider)]string name, IEventCallback callback)
		{
			return Connection.GetService<IEventService>().Trigger(MicroService.Token, name, callback);
		}
	}
}