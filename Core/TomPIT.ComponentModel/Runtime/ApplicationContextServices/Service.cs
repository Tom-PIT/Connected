using System;
using TomPIT.ComponentModel;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public abstract class Service : ApplicationContextClient
	{
		public Service(IApplicationContext context) : base(context)
		{
		}

		protected IApi GetApi(Guid microService, string api, bool explicitIdentifier)
		{
			var component = Context.GetServerContext().GetService<IComponentService>().SelectComponent(microService, "Api", api);

			if (component == null)
			{
				if (!explicitIdentifier)
					component = Context.GetServerContext().GetService<IComponentService>().SelectComponent("Api", api);

				if (component == null)
					throw RuntimeException.Create(Context, string.Format("{0} ({1})", SR.ErrComponentNotFound, api), CreateDescriptor(RuntimeEvents.DataRead, ExecutionContext.Api, null, null, microService));
			}

			if (!(Context.GetServerContext().GetService<IComponentService>().SelectConfiguration(component.Token) is IApi config))
				throw RuntimeException.Create(Context, string.Format("{0} ({1})", SR.ErrComponentCorrupted, api), CreateDescriptor(RuntimeEvents.DataRead, ExecutionContext.Api, null, null, microService));

			return config;
		}
	}
}
