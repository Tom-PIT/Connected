using System;
using System.Collections.Generic;
using TomPIT.BigData;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Data.BigData
{
	internal class BigDataPartitionFieldStatistics : SynchronizedRepository<IPartitionFieldStatistics, string>
	{
		public BigDataPartitionFieldStatistics(IMemoryCache container) : base(container, "bigdatafieldstatistics")
		{
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.QueryFieldStatistics();

			foreach (var i in ds)
				Set(GenerateKey(i.File, i.FieldName), i, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');
			var file = DataModel.BigDataPartitionFiles.Select(new Guid(tokens[0]));

			if (file == null)
				throw new SysException(SR.ErrBigDataFileNotFound);

			var r = Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.SelectFieldStatistics(file, tokens[1]);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IPartitionFieldStatistics Select(Guid file, string fileName)
		{
			return Get(GenerateKey(file, fileName));
		}

		public List<IPartitionFieldStatistics> Query()
		{
			return All();
		}

		public void Update(Guid file, string fieldName, string startString, string endString, double startNumber, double endNumber, DateTime startDate, DateTime endDate)
		{
			var f = DataModel.BigDataPartitionFiles.Select(file);

			if (f == null)
				throw new SysException(SR.ErrBigDataFileNotFound);

			Shell.GetService<IDatabaseService>().Proxy.BigData.Partitions.UpdateFieldStatistics(f, fieldName, startString, endString, startNumber, endNumber, startDate, endDate);
			Refresh(GenerateKey(file, fieldName));
			BigDataNotifications.PartitionFieldStatisticsChanged(file, fieldName);
		}

		private string GenerateKey(Guid file, string fieldName)
		{
			return $"{file}.{fieldName}";
		}
	}
}
