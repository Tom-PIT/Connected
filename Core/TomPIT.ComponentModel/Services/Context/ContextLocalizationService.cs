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
			var config = Context.Connection().GetService<IComponentService>().SelectConfiguration(Context.MicroService.Token, "StringTable", stringTable) as IStringTable;
			var str = config.Strings.FirstOrDefault(f => string.Compare(f.Key, key, true) == 0);

			if (str == null)
				throw new RuntimeException($"{SR.ErrStringResourceNotFound} ({stringTable}/{key})").WithMetrics(Context);

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
