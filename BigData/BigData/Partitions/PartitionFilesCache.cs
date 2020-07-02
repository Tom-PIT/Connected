using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.BigData.Partitions
{
	internal class PartitionFilesCache : SynchronizedClientRepository<IPartitionFile, Guid>
	{
		public PartitionFilesCache(ITenant tenant) : base(tenant, "partitionfiles")
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
			if (key == null)
				key = string.Empty;

			var candidates = Where(f => f.Partition == partition && (string.Compare(f.Key, key, true) == 0));

			if (candidates.Count == 0 || (startTimestamp == DateTime.MinValue && endTimestamp == DateTime.MinValue))
				return candidates;

			var result = new List<IPartitionFile>();

			foreach (var candidate in candidates)
			{
				if (IntersectsWith(startTimestamp, endTimestamp, candidate.StartTimestamp, candidate.EndTimestamp))
					result.Add(candidate);
			}

			return result;
		}

		private bool IntersectsWith(DateTime startValue, DateTime endValue, DateTime start, DateTime end)
		{
			if (end == DateTime.MinValue)
				end = DateTime.MaxValue;

			if (endValue == DateTime.MinValue)
				endValue = DateTime.MaxValue;

			if ((startValue < start && endValue < start)
				|| (startValue > end))
				return false;

			return true;
		}

		public List<IPartitionFile> Where(List<Guid> files)
		{
			return Where(f => files.Any(g => g == f.FileName));
		}
		protected override void OnInitializing()
		{
			var u = Tenant.CreateUrl("BigDataManagement", "QueryFiles");
			var files = Tenant.Get<List<PartitionFile>>(u);

			foreach (var file in files)
				Set(file.FileName, file, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var u = Tenant.CreateUrl("BigDataManagement", "SelectFile");
			var e = new JObject
			{
				{"token", id }
			};

			var file = Tenant.Post<PartitionFile>(u, e);

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