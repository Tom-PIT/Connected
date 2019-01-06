using System;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class Localization : ApplicationContextClient, ILocalizationService
	{
		public Localization(IApplicationContext context) : base(context)
		{
			if (!context.Services.Identity.IsAuthenticated)
				return;

			Language = context.Services.Identity.User.Language;
		}

		public Guid Language { get; }
	}
}
