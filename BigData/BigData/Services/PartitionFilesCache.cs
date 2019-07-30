using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.BigData.Services
{
	internal class PartitionFilesCache : SynchronizedClientRepository<IPartitionFile, Guid>
	{
		public PartitionFilesCache(ISysConnection connection) : base(connection, "partitionfiles")
		{
		}

		public IPartitionFile Select(Guid fileName)
		{
			return Get(fileName);
		}

		public List<IPartitionFile> Query(Guid partition)
		{
			return Where(f => f.Partition == partition);
		}

		public List<IPartitionFile> Query(Guid partition, string key, DateTime startTimestamp, DateTime endTimestamp)
		{
			return Where(f => f.Partition == partition
				&& (string.IsNullOrWhiteSpace(key) || string.Compare(f.Key, key, true) == 0)
				&& (startTimestamp == DateTime.MinValue || f.StartTimestamp <= startTimestamp)
				&& (endTimestamp == DateTime.MinValue || f.Status == PartitionFileStatus.Open || endTimestamp <= f.EndTimestamp));
		}
		protected override void OnInitializing()
		{
			var u = Connection.CreateUrl("BigDataManagement", "QueryFiles");
			var files = Connection.Get<List<PartitionFile>>(u);

			foreach (var file in files)
				Set(file.FileName, file, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Connection.CreateUrl("BigDataManagement", "SelectFile");
			var e = new JObject
			{
				{"token", id }
			};

			var file = Connection.Post<PartitionFile>(u, e);

			if (file != null)
				Set(file.FileName, file, TimeSpan.Zero);
		}

		public void Notify(Guid id, bool remove = false)
		{
			if (remove)
				Remove(id);
			else
				Refresh(id);
		}
	}
}