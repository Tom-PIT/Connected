using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Globalization;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Globalization
{
	internal class LanguagesModel : SynchronizedRepository<ILanguage, Guid>
	{
		public LanguagesModel(IMemoryCache container) : base(container, "language")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Globalization.Languages.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Globalization.Languages.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public ILanguage Select(int lcid)
		{
			return Get(f => f.Lcid == lcid);
		}

		public ILanguage Select(Guid token)
		{
			return Get(token,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					return Shell.GetService<IDatabaseService>().Proxy.Globalization.Languages.Select(token);
				});
		}

		public ImmutableList<ILanguage> Query()
		{
			return All();
		}

		public Guid Insert(string name, int lcid, LanguageStatus status, string mappings)
		{
			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Globalization.Languages.Insert(token, name, lcid, status, mappings);

			Refresh(token);
			CachingNotifications.LanguageChanged(token);

			return token;
		}

		public void Update(Guid token, string name, int lcid, LanguageStatus status, string mappings)
		{
			var l = Select(token);

			if (l == null)
				throw new SysException(SR.ErrLanguageNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Globalization.Languages.Update(l, name, lcid, status, mappings);

			Refresh(token);
			CachingNotifications.LanguageChanged(token);
		}

		public void Delete(Guid token)
		{
			var l = Select(token);

			if (l == null)
				throw new SysException(SR.ErrLanguageNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Globalization.Languages.Delete(l);

			Remove(token);
			CachingNotifications.LanguageRemoved(token);
		}
	}
}
