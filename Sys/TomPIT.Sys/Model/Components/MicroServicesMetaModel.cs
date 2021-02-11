using System;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Model.Components
{
	internal class MicroServicesMetaModel : CacheRepository<MicroServiceMeta, Guid>
	{
		public MicroServicesMetaModel(IMemoryCache container) : base(container, "microservicemeta")
		{

		}

		public MicroServiceMeta Select(Guid microService)
		{
			return Get(microService,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					var s = DataModel.MicroServices.Select(microService);

					if (s == null)
						throw SysException.MicroServiceNotFound();

					var r = Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.SelectMeta(s);

					if (r == null)
						return null;

					var m = new MicroServiceMeta();

					m.Content = r;

					return m;
				});
		}

		public void Update(Guid microService, byte[] meta)
		{
			var s = DataModel.MicroServices.Select(microService);

			if (s == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.MicroServices.UpdateMeta(s, meta);
			Invalidate(microService);
		}

		public void Invalidate(Guid microService)
		{
			Refresh(microService);
		}
	}
}