using System;
using System.Globalization;
using System.Linq;
using System.Threading;
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

			var language = Context.Connection().GetService<ILanguageService>().Select(Thread.CurrentThread.CurrentUICulture.LCID);

			if (language != null && language.Status == LanguageStatus.Visible)
				Language = language.Token;
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
				lcid = Thread.CurrentThread.CurrentUICulture.LCID;

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
