using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Caching;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.BigData.Partitions
{
	internal class PartitionFieldStatisticsCache : SynchronizedClientRepository<IPartitionFieldStatistics, string>
	{
		public PartitionFieldStatisticsCache(ITenant tenant) : base(tenant, "partitionfieldstats")
		{
		}

		public IPartitionFieldStatistics Select(Guid file, string fieldName)
		{
			return Get(GenerateKey(file, fieldName));
		}

		protected override void OnInitializing()
		{
			var u = Tenant.CreateUrl("BigDataManagement", "QueryFieldStatistics");
			var fields = Tenant.Get<List<PartitionFieldStatistics>>(u);

			foreach (var field in fields)
				Set(GenerateKey(field.File, field.FieldName), field, TimeSpan.Zero);
		}

		protected override void OnInvalidate(string id)
		{
			var tokens = id.Split('.');

			var u = Tenant.CreateUrl("BigDataManagement", "SelectFieldStatistic");
			var e = new JObject
			{
				{"file", tokens[0] },
				{"fieldName", tokens[1] }
			};

			var field = Tenant.Post<PartitionFieldStatistics>(u, e);

			if (field != null)
				Set(GenerateKey(field.File, field.FieldName), field, TimeSpan.Zero);
		}

		public List<Guid> Query(List<Guid> filtered, Guid partition, string key, string index, Type type, List<object> values)
		{
			var candidates = QueryCandidates(filtered, partition, key, index);
			var result = new List<Guid>();

			foreach (var candidate in candidates)
			{
				foreach (var value in values)
				{
					if (Hit(candidate, type, value))
					{
						result.Add(candidate.File);
						break;
					}
				}
			}

			return result;
		}

		public List<Guid> Query(List<Guid> filtered, Guid partition, string key, string index, Type type, object value)
		{
			var candidates = QueryCandidates(filtered, partition, key, index);
			var result = new List<Guid>();

			foreach (var candidate in candidates)
			{
				if (Hit(candidate, type, value))
					result.Add(candidate.File);
			}

			return result;
		}

		public List<Guid> Query(List<Guid> filtered, Guid partition, string key, string index, Type type, object start, object end)
		{
			var candidates = QueryCandidates(filtered, partition, key, index);
			var result = new List<Guid>();

			foreach (var candidate in candidates)
			{
				if (Hit(candidate, type, start, end))
					result.Add(candidate.File);
			}

			return result;
		}

		private bool Hit(IPartitionFieldStatistics field, Type type, object start, object end)
		{
			if (type == typeof(DateTime))
			{
				var s = Types.Convert<DateTime>(start);
				var e = Types.Convert<DateTime>(end);

				if (field.StartDate == DateTime.MinValue && field.EndDate == DateTime.MinValue)
					return false;

				if (s == DateTime.MinValue)
				{
					if (field.EndDate >= e)
						return true;
				}
				else if (e == DateTime.MinValue)
				{
					if (field.StartDate <= s)
						return true;
				}
				else
				{
					if (field.StartDate <= s && field.EndDate >= e)
						return true;
				}
			}
			else if (type.IsNumericType())
			{
				var s = Types.Convert<decimal>(start);
				var e = Types.Convert<decimal>(end);

				if (field.StartNumber == decimal.MinValue && field.EndNumber == decimal.MinValue)
					return false;

				if (s == decimal.Zero)
				{
					if (field.EndNumber >= e)
						return true;
				}
				else if (e == decimal.Zero)
				{
					if (field.StartNumber <= s)
						return true;
				}
				else
				{
					if (field.StartNumber <= s && field.EndNumber >= e)
						return true;
				}
			}
			else
			{
				var s = Types.Convert<string>(start);
				var e = Types.Convert<string>(end);

				if (string.IsNullOrWhiteSpace(field.StartString) && string.IsNullOrWhiteSpace(field.EndString))
					return false;

				if (string.IsNullOrWhiteSpace(s))
				{
					if (string.Compare(field.EndString, e, true) >= 0)
						return true;
				}
				else if (string.IsNullOrWhiteSpace(e))
				{
					if (string.Compare(field.StartString, s, true) <= 0)
						return true;
				}
				else
				{
					if (string.Compare(field.StartString, s, true) <= 0 && string.Compare(field.EndString, e, true) >= 0)
						return true;
				}
			}

			return false;
		}
		private bool Hit(IPartitionFieldStatistics field, Type type, object value)
		{
			if (type == typeof(DateTime))
			{
				var v = Types.Convert<DateTime>(value);

				if (field.StartDate == DateTime.MinValue || field.EndDate == DateTime.MinValue)
					return false;

				if (field.StartDate <= v && field.EndDate >= v)
					return true;
			}
			else if (type.IsNumericType())
			{
				var v = Types.Convert<decimal>(value);

				if (field.StartNumber == decimal.MinValue || field.EndNumber == decimal.MinValue)
					return false;

				if (field.StartNumber <= v && field.EndNumber >= v)
					return true;
			}
			else
			{
				var v = Types.Convert<string>(value);

				if (string.IsNullOrWhiteSpace(field.StartString) || string.IsNullOrWhiteSpace(field.EndString))
					return false;

				if (string.Compare(field.StartString, v, true) <= 0 && string.Compare(field.EndString, v, true) >= 0)
					return true;
			}

			return false;
		}

		private ImmutableList<IPartitionFieldStatistics> QueryCandidates(List<Guid> files, Guid partition, string key, string index)
		{
			return Where(f => (files.Count == 0 || files.Any(g => g == f.File)) && f.Partition == partition && string.Compare(f.Key, key, true) == 0 && string.Compare(f.FieldName, index, true) == 0);
		}

		public void Notify(Guid file, string fieldName, bool remove = false)
		{
			var key = GenerateKey(file, fieldName);

			if (remove)
				Remove(key);
			else
				Refresh(key);
		}
	}
}