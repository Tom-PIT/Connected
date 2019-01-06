using System;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Exceptions;

namespace TomPIT.Services.Context
{
	public abstract class ContextService : ContextClient
	{
		public ContextService(IExecutionContext context) : base(context)
		{
		}

		protected IApi GetApi(Guid microService, string api, bool explicitIdentifier)
		{
			var component = Context.Connection().GetService<IComponentService>().SelectComponent(microService, "Api", api);

			if (component == null)
			{
				if (!explicitIdentifier)
					component = Context.Connection().GetService<IComponentService>().SelectComponent("Api", api);

				if (component == null)
					throw ExecutionException.Create(Context, string.Format("{0} ({1})", SR.ErrComponentNotFound, api), CreateDescriptor(ExecutionEvents.DataRead, ExecutionContextState.Api, null, null, microService));
			}

			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is IApi config))
				throw ExecutionException.Create(Context, string.Format("{0} ({1})", SR.ErrComponentCorrupted, api), CreateDescriptor(ExecutionEvents.DataRead, ExecutionContextState.Api, null, null, microService));

			return config;
		}
	}
}
