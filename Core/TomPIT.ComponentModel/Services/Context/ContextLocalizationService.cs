using System;

namespace TomPIT.Services.Context
{
	internal class ContextLocalizationService : ContextClient, IContextLocalizationService
	{
		public ContextLocalizationService(IExecutionContext context) : base(context)
		{
			if (!context.Services.Identity.IsAuthenticated)
				return;

			Language = context.Services.Identity.User.Language;
		}

		public Guid Language { get; }
	}
}
