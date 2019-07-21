using System;
using System.Globalization;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Globalization;

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

		public string GetString(string stringTable, string key, int lcid)
		{
			return GetString(stringTable, key, lcid, true);
		}
		private string GetString(string stringTable, string key, int lcid, bool throwException)
		{
			var microService = Context.MicroService;
			var st = stringTable;

			if (st.Contains('/'))
			{
				var tokens = st.Split('/');

				Context.MicroService.ValidateMicroServiceReference(Context.Connection(), tokens[0]);

				microService = Context.Connection().GetService<IMicroServiceService>().Select(tokens[0]);

				if (microService == null)
					throw new Exception($"{SR.ErrMicroServiceNotFound} ({microService})");

				st = tokens[1];
			}

			if (lcid == 0)
			{
				if (Language == Guid.Empty)
					lcid = CultureInfo.InvariantCulture.LCID;
				else
				{
					var language = Context.Connection().GetService<ILanguageService>().Select(Language);

					lcid = language == null
						? CultureInfo.InvariantCulture.LCID
						: language.Lcid;
				}
			}

			return Context.Connection().GetService<ILocalizationService>().GetString(microService.Name, st, key, lcid, throwException);
		}

		public string GetString(string stringTable, string key)
		{
			return GetString(stringTable, key, 0);
		}

		public string TryGetString(string stringTable, string key)
		{
			return GetString(stringTable, key, 0, false);
		}

		public string TryGetString(string stringTable, string key, int lcid)
		{
			return GetString(stringTable, key, lcid, false);
		}
	}
}
