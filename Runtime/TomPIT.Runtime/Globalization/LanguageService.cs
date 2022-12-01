using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Collections;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Globalization
{
	internal class LanguageService : SynchronizedClientRepository<ILanguage, Guid>, ILanguageService, ILanguageNotification
	{
		private readonly Lazy<ConcurrentDictionary<string, ILanguage>> _mappings = new(() => { return new ConcurrentDictionary<string, ILanguage>(StringComparer.OrdinalIgnoreCase); });
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
			}
		}

		protected override void OnInitialized()
        {
            ApplySupportedCultures();
            ResetMappings();
		}

		protected override void OnInvalidate(Guid id)
		{
			Remove(id);

			var u = Tenant.CreateUrl("Language", "Select")
			.AddParameter("language", id);

			var r = Tenant.Get<Language>(u);

			if (r != null)
				Set(r.Token, r, TimeSpan.Zero);
			
			ApplySupportedCultures();

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
            ApplySupportedCultures();
        }

		public void NotifyRemoved(object sender, LanguageEventArgs e)
		{
			Remove(e.Language);
			ApplySupportedCultures();
		}

		public void ApplySupportedCultures() 
		{
			if (Instance.RequestLocalizationOptions is null)
				return;

			lock (Instance.RequestLocalizationOptions)
			{
				var languages = All();
				var cultures = languages
					.Where(e => e.Status == LanguageStatus.Visible)
					.Select(e => GetCulture(e))
					.Where(e => e is not null)
					.ToImmutableList(true);

				foreach (var culture in cultures)
				{
					if (!Instance.RequestLocalizationOptions.SupportedCultures.Contains(culture))
						Instance.RequestLocalizationOptions.SupportedCultures.Add(culture);

					if (!Instance.RequestLocalizationOptions.SupportedUICultures.Contains(culture))
						Instance.RequestLocalizationOptions.SupportedUICultures.Add(culture);
				}

				var supportedCultures = Instance.RequestLocalizationOptions.SupportedCultures.ToImmutableList(true);
				foreach (var culture in supportedCultures)
				{
					if (!cultures.Contains(culture))
					{
						Instance.RequestLocalizationOptions.SupportedCultures.Remove(culture);
						Instance.RequestLocalizationOptions.SupportedUICultures.Remove(culture);
					}
				}
			}
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