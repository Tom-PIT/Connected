using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;

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
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrInvalidMicroServiceQualifier, token.ToString())).WithMetrics(Context);
		}

		protected void ParseMicroService(string name)
		{
			MicroService = Context.Connection().GetService<IMicroServiceService>().Select(name);

			if (MicroService == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrInvalidMicroServiceQualifier, name)).WithMetrics(Context);
		}

		protected void ParseMicroService()
		{
			if (MicroService != null)
				return;

			if (Context.MicroService == null)
				throw new RuntimeException(SR.ErrCannotResolveMicroService).WithMetrics(Context);

			MicroService = Context.MicroService;
		}

		protected virtual void Validate()
		{

		}
	}
}

