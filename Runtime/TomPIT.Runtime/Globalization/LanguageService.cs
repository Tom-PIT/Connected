using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Globalization
{
	internal class LanguageService : SynchronizedClientRepository<ILanguage, Guid>, ILanguageService, ILanguageNotification
	{
		private Lazy<ConcurrentDictionary<string, ILanguage>> _mappings = new Lazy<ConcurrentDictionary<string, ILanguage>>(()=> { return new ConcurrentDictionary<string, ILanguage>(StringComparer.OrdinalIgnoreCase); });
		public LanguageService(ITenant tenant) : base(tenant, "language")
		{
			Initialize();
		}

		private ConcurrentDictionary<string, ILanguage> Mappings => _mappings.Value;

		protected override void OnInitializing()
		{
			var u = Tenant.CreateUrl("Language", "Query");
			var languages = Tenant.Get<List<Language>>(u).ToList<ILanguage>();

			foreach (var language in languages)
			{
				Set(language.Token, language, TimeSpan.Zero);
				AddCulture(language);
			}
		}

		protected override void OnInitialized()
		{
			ResetMappings();
		}

		protected override void OnInvalidate(Guid id)
		{
			RemoveCulture(Select(id));

			var u = Tenant.CreateUrl("Language", "Select")
			.AddParameter("language", id);

			var r = Tenant.Get<Language>(u);

			if (r != null)
			{
				Set(r.Token, r, TimeSpan.Zero);
				AddCulture(r);
			}

			ResetMappings();
		}

		public ImmutableList<ILanguage> Query()
		{
			return All();
		}

		public ILanguage Select(Guid language)
		{
			return Get(language);
		}

		public ILanguage Match(string languageString)
		{
			if (!Mappings.TryGetValue(languageString, out ILanguage language))
				return null;

			return language;
		}

		public ILanguage Select(int lcid)
		{
			return Get(f => f.Lcid == lcid);
		}

		public void NotifyChanged(object sender, LanguageEventArgs e)
		{
			Refresh(e.Language);
		}

		public void NotifyRemoved(object sender, LanguageEventArgs e)
		{
			RemoveCulture(Select(e.Language));
			Remove(e.Language);
		}

		private void RemoveCulture(ILanguage language)
		{
			var culture = GetCulture(language);

			if (culture == null)
				return;

			if (Instance.RequestLocalizationOptions.SupportedCultures.Contains(culture))
			{
				Instance.RequestLocalizationOptions.SupportedCultures.Remove(culture);
				Instance.RequestLocalizationOptions.SupportedUICultures.Remove(culture);
			}
		}

		private void AddCulture(ILanguage language)
		{
			if (language == null || language.Status == LanguageStatus.Hidden)
				return;

			var culture = GetCulture(language);

			if (culture == null)
				return;

			if (!Instance.RequestLocalizationOptions.SupportedCultures.Contains(culture))
				Instance.RequestLocalizationOptions.SupportedCultures.Add(culture);

			if (!Instance.RequestLocalizationOptions.SupportedUICultures.Contains(culture))
				Instance.RequestLocalizationOptions.SupportedUICultures.Add(culture);
		}

		private CultureInfo GetCulture(ILanguage language)
		{
			if (language == null || language.Lcid == 0 || language.Lcid == CultureInfo.InvariantCulture.LCID)
				return null;

			return CultureInfo.GetCultureInfo(language.Lcid);
		}

		private void ResetMappings()
		{
			Mappings.Clear();

			foreach(var language in All())
			{
				if (string.IsNullOrWhiteSpace(language.Mappings) || language.Status == LanguageStatus.Hidden)
					continue;

				var tokens = language.Mappings.Split(',');

				foreach(var token in tokens)
				{
					if (string.IsNullOrWhiteSpace(token))
						continue;

					Mappings.TryAdd(token, language);
				}
			}
		}
	}
}