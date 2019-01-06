using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;

namespace TomPIT.Services.Context
{
	public abstract class Qualifier : ContextClient
	{
		public Qualifier(IExecutionContext context, IMicroService microService, string value) : base(context)
		{
			Value = value;
			MicroService = microService;

			Segments = value == null || value.Length == 0
				? new List<string>()
				: value.Trim('/').Split('/').ToList();

			Initialize();
			Validate();
		}

		public Qualifier(IExecutionContext context, string value) : this(context, null, value)
		{
		}

		protected abstract void Initialize();

		protected string Value { get; }
		public IMicroService MicroService { get; private set; }
		protected List<string> Segments { get; private set; }


		protected void ParseMicroService(Guid token)
		{
			MicroService = Context.Connection().GetService<IMicroServiceService>().Select(token);

			if (MicroService == null)
				throw ExecutionException.InvalidMicroServiceQualifier(Context, CreateDescriptor(ExecutionEvents.Runtime), token.ToString());
		}

		protected void ParseMicroService(string name)
		{
			MicroService = Context.Connection().GetService<IMicroServiceService>().Select(name);

			if (MicroService == null)
				throw ExecutionException.InvalidMicroServiceQualifier(Context, CreateDescriptor(ExecutionEvents.Runtime), name);
		}

		protected void ParseMicroService()
		{
			if (MicroService != null)
				return;

			if (Context.MicroService() == Guid.Empty)
				throw ExecutionException.CannotResolveMicroService(Context, CreateDescriptor(ExecutionEvents.Runtime));

			MicroService = Context.Connection().GetService<IMicroServiceService>().Select(Context.MicroService());
		}

		protected virtual void Validate()
		{

		}
	}
}

