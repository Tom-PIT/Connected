using System;
using System.Globalization;
using System.Linq;
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

			var cacheKey = $"stringTable{microService.Token}{st}";

			if (!(Shell.HttpContext.Items[cacheKey] is IStringTable config))
				config = Context.Connection().GetService<IComponentService>().SelectConfiguration(microService.Token, "StringTable", st) as IStringTable;

			Shell.HttpContext.Items[cacheKey] = config;

			var str = config.Strings.FirstOrDefault(f => string.Compare(f.Key, key, true) == 0);

			if (str == null)
				throw new RuntimeException($"{SR.ErrStringResourceNotFound} ({st}/{key})").WithMetrics(Context);

			if (!str.IsLocalizable)
				return str.DefaultValue;

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

			if (lcid == CultureInfo.InvariantCulture.LCID)
				return str.DefaultValue;

			var translation = str.Translations.FirstOrDefault(f => f.Lcid == lcid);

			return translation == null
				? GetFallbackString(str, lcid)
				: translation.Value;
		}

		private string GetFallbackString(IStringResource str, int lcid)
		{
			var culture = CultureInfo.GetCultureInfo(lcid);

			if (culture.LCID == CultureInfo.InvariantCulture.LCID || culture.Parent == null)
				return str.DefaultValue;

			var parent = culture.Parent;

			var translation = str.Translations.FirstOrDefault(f => f.Lcid == parent.LCID);

			if (translation != null)
				return translation.Value;

			return GetFallbackString(str, parent.LCID);
		}

		public string GetString(string stringTable, string key)
		{
			return GetString(stringTable, key, 0);
		}
	}
}
