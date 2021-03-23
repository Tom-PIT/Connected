using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Globalization;
using TomPIT.Globalization;

namespace TomPIT.Ide.Environment.Providers
{
	internal class GlobalizationProvider : EnvironmentObject, IGlobalizationProvider
	{
		private ImmutableList<ILanguage> _languages = null;
		private const string Default = "Default";

		public GlobalizationProvider(IEnvironment environment) : base(environment)
		{
			if (Environment.RequestBody != null)
				Language = Environment.RequestBody.Optional("language", Default);

			if (string.IsNullOrWhiteSpace(Language))
				Language = Default;
		}

		public Guid LanguageToken
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Language) || string.Compare(Language, Default, true) == 0)
					return Guid.Empty;

				var d = LanguageList.FirstOrDefault(f => string.Compare(f.Name, Language, true) == 0);

				if (d == null)
					return Guid.Empty;

				return d.Token;
			}
		}

		public string Language { get; }

		private ImmutableList<ILanguage> LanguageList
		{
			get
			{
				if (_languages == null)
					_languages = Environment.Context.Tenant.GetService<ILanguageService>().Query();

				return _languages;
			}
		}

		public List<string> Languages
		{
			get
			{
				var r = LanguageList.Select(f => f.Name).ToList();

				r.Insert(0, Default);

				return r;
			}
		}
	}
}
