using System;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Printing;

namespace TomPIT.Sys.Model.Printing
{
	public class PrintingSerialNumbersModel : SynchronizedRepository<ISerialNumber, string>
	{
		public PrintingSerialNumbersModel(IMemoryCache container) : base(container, "printSerialNumber")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Printing.QuerySerialNumbers();

			foreach (var i in ds)
				Set(i.Category.ToLowerInvariant(), i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Printing.SelectSerialNumber(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id.ToLowerInvariant(), r, TimeSpan.Zero);
		}

		public long Next(string category)
		{
			if (string.IsNullOrWhiteSpace(category))
				return 0;

			var existing = Ensure(category);

			existing.Increment();

			Shell.GetService<IDatabaseService>().Proxy.Printing.UpdateSerialNumber(existing);

			return existing.SerialNumber;
		}

		private ISerialNumber Ensure(string category)
		{
			var existing = Get(category.ToLowerInvariant());

			if (existing == null)
			{
				Shell.GetService<IDatabaseService>().Proxy.Printing.InsertSerialNumber(category, 0);

				Refresh(category);

				existing = Get(category.ToLowerInvariant());
			}

			return existing;
		}
	}
}
