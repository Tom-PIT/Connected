using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data
{
	internal class MicroServiceStrings : SynchronizedRepository<IMicroServiceString, string>
	{
		public MicroServiceStrings(IMemoryCache container) : base(container, "microservicestrings")
		{

		}

		public List<IMicroServiceString> Query(Guid microService)
		{
			return Where(f => f.MicroService == microService);
		}

		public List<IMicroServiceString> Query(Guid microService, Guid language)
		{
			return Where(f => f.MicroService == microService && f.Language == language);
		}

		public IMicroServiceString Select(Guid microService, Guid language, Guid element, string property)
		{
			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var l = DataModel.Languages.Select(language);

			if (l == null)
				throw new SysException(SR.ErrLanguageNotFound);

			var key = GenerateKey(microService, language, element, property);

			return Get(key,
				(f) =>
				{
					f.AllowNull = true;
					f.Duration = TimeSpan.Zero;

					return Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.SelectString(s, l, element, property);
				});
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split(".");

			var s = DataModel.MicroServices.Select(tokens[0].AsGuid());

			if (s == null)
			{
				Remove(id);
				return;
			}

			var l = DataModel.Languages.Select(tokens[1].AsGuid());

			if (l == null)
			{
				Remove(id);
				return;
			}

			var r = Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.SelectString(s, l, tokens[2].AsGuid(), tokens[3]);

			Set(id, r, TimeSpan.Zero);
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.QueryStrings();

			foreach (var i in ds)
				Set(GenerateKey(i.MicroService, i.Language, i.Element, i.Property), i, TimeSpan.Zero);
		}

		public void Refresh(Guid microService, Guid language, Guid element, string property)
		{
			Refresh(GenerateKey(microService, language, element, property));
		}

		public void Update(Guid microService, Guid language, Guid element, string property, string value)
		{
			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw SysException.MicroServiceNotFound();

			var l = DataModel.Languages.Select(language);

			if (l == null)
				throw new SysException(SR.ErrLanguageNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.UpdateString(s, l, element, property, value);

			Refresh(microService, language, element, property);
			CachingNotifications.MicroServiceStringChanged(microService, language, element, property);
		}

		public void Delete(Guid microService, Guid element, string property)
		{
			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw SysException.MicroServiceNotFound();

			Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.DeleteString(s, element, property);
			CachingNotifications.MicroServiceStringRemoved(microService, element, property);
		}
	}
}
