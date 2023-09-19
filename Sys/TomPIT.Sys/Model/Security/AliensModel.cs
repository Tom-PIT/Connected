using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Globalization;
using TomPIT.Security;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.Security
{
	public class AliensModel : SynchronizedRepository<IAlien, Guid>
	{
		public AliensModel(IMemoryCache container) : base(container, "alien")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Security.Aliens.Query();

			foreach (var i in ds)
				Set(i.Token, i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Security.Aliens.Select(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IAlien Select(Guid token)
		{
			return Get(token);
		}

		public IAlien Select(string resourceType, string resourcePrimaryKey)
		{
			return Get(f => string.Compare(f.ResourceType, resourceType, true) == 0 && string.Compare(f.ResourcePrimaryKey, resourcePrimaryKey, true) == 0);
		}

		public IAlien Select(string email)
		{
			return Get(f => string.Compare(f.Email, email, true) == 0);
		}

		public IAlien SelectByMobile(string mobile)
		{
			return Get(f => string.Compare(f.Mobile, mobile, true) == 0);
		}

		public IAlien SelectByPhone(string phone)
		{
			return Get(f => string.Compare(f.Phone, phone, true) == 0);
		}

		public Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType, string resourcePrimaryKey)
		{
			ILanguage l = null;

			if (language != Guid.Empty)
			{
				l = DataModel.Languages.Select(language);

				if (l == null)
					throw new SysException(SR.ErrLanguageNotFound);
			}

			var token = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Security.Aliens.Insert(token, firstName, lastName, email, mobile, phone, l, timezone, resourceType, resourcePrimaryKey);

			Refresh(token);
			CachingNotifications.AlienChanged(token);

			return token;
		}

		public void Update(Guid token, string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType, string resourcePrimaryKey)
		{
			var alien = Select(token);

			if (alien == null)
				throw new SysException(SR.ErrAlienNotFound);

			ILanguage l = null;

			if (language != Guid.Empty)
			{
				l = DataModel.Languages.Select(language);

				if (l == null)
					throw new SysException(SR.ErrLanguageNotFound);
			}

			Shell.GetService<IDatabaseService>().Proxy.Security.Aliens.Update(alien, firstName, lastName, email, mobile, phone, l, timezone, resourceType, resourcePrimaryKey);

			Refresh(alien.Token);
			CachingNotifications.AlienChanged(alien.Token);
		}

		public ImmutableList<IAlien> Query()
		{
			return All();
		}

		public void Delete(Guid token)
		{
			var alien = Select(token);

			if (alien == null)
				throw new SysException(SR.ErrAlienNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Security.Aliens.Delete(alien);

			Remove(token);
			CachingNotifications.AlienChanged(token);
		}
	}
}