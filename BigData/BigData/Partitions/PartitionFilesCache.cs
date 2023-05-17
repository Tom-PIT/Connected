using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;

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

		public ImmutableList<IPartitionFile> Query(Guid partition)
		{
			return Where(f => f.Partition == partition);
		}

		public ImmutableList<IPartitionFile> Query(Guid partition, Guid timezone, string key, DateTime startTimestamp, DateTime endTimestamp)
		{
			if (key is null)
				key = string.Empty;

			var candidates = Where(f => f.Partition == partition && f.Timezone == timezone && (string.Compare(f.Key, key, true) == 0));

			if (candidates.Count == 0 || (startTimestamp == DateTime.MinValue && endTimestamp == DateTime.MinValue))
				return candidates;

			var result = new List<IPartitionFile>();

			foreach (var candidate in candidates)
			{
				if (IntersectsWith(startTimestamp, endTimestamp, candidate.StartTimestamp, candidate.EndTimestamp))
					result.Add(candidate);
			}

			return result.ToImmutableList();
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

		public ImmutableList<IPartitionFile> Where(List<Guid> files)
		{
			return Where(f => files.Any(g => g == f.FileName));
		}
		protected override void OnInitializing()
		{
			var files = Instance.SysProxy.Management.BigData.QueryFiles();

			foreach (var file in files)
				Set(file.FileName, file, TimeSpan.Zero);
		}

		protected override void OnInvalidate(Guid id)
		{
			var file = Instance.SysProxy.Management.BigData.SelectFile(id);

			if (file is not null)
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