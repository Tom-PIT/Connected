using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public abstract class Qualifier : ApplicationContextClient
	{
		public Qualifier(IApplicationContext context, IMicroService microService, string value) : base(context)
		{
			Value = value;
			MicroService = microService;

			Segments = value == null || value.Length == 0
				? new List<string>()
				: value.Trim('/').Split('/').ToList();

			Initialize();
			Validate();
		}

		public Qualifier(IApplicationContext context, string value) : this(context, null, value)
		{
		}

		protected abstract void Initialize();

		protected string Value { get; }
		public IMicroService MicroService { get; private set; }
		protected List<string> Segments { get; private set; }


		protected void ParseMicroService(Guid token)
		{
			MicroService = Context.GetServerContext().GetService<IMicroServiceService>().Select(token);

			if (MicroService == null)
				throw RuntimeException.InvalidMicroServiceQualifier(Context, CreateDescriptor(RuntimeEvents.Runtime), token.ToString());
		}

		protected void ParseMicroService(string name)
		{
			MicroService = Context.GetServerContext().GetService<IMicroServiceService>().Select(name);

			if (MicroService == null)
				throw RuntimeException.InvalidMicroServiceQualifier(Context, CreateDescriptor(RuntimeEvents.Runtime), name);
		}

		protected void ParseMicroService()
		{
			if (MicroService != null)
				return;

			if (Context.MicroService() == Guid.Empty)
				throw RuntimeException.CannotResolveMicroService(Context, CreateDescriptor(RuntimeEvents.Runtime));

			MicroService = Context.GetServerContext().GetService<IMicroServiceService>().Select(Context.MicroService());
		}

		protected virtual void Validate()
		{

		}
	}
}

