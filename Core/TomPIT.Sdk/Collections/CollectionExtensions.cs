using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Exceptions;

namespace TomPIT.Collections
{
	public static class CollectionExtensions
	{
		public static Dictionary<string, string> ToDictionary(this IEnumerable items, string keyProperty, string valueProperty)
		{
			var result = new Dictionary<string, string>();

			foreach (var item in items)
			{
				var keyPropertyInfo = item.GetType().GetProperty(keyProperty);
				var valuePropertyInfo = item.GetType().GetProperty(valueProperty);

				if (keyPropertyInfo == null)
					throw new RuntimeException($"SR.ErrPropertyNotFound ({keyProperty})");

				if (valuePropertyInfo == null)
					throw new RuntimeException($"SR.ErrPropertyNotFound ({valueProperty})");

				var keyValue = Types.Convert<string>(keyPropertyInfo.GetValue(item));
				var valueValue = Types.Convert<string>(valuePropertyInfo.GetValue(item));

				result.Add(keyValue, valueValue);
			}

			return result;
		}

		public static ImmutableArray<TSource> ToImmutableArray<TSource>(this IEnumerable<TSource> items, bool performLock)
		{
			if (!performLock)
				return items.ToImmutableArray();

			lock (items)
				return items.ToImmutableArray();
		}

		public static ImmutableList<TSource> ToImmutableList<TSource>(this IEnumerable<TSource> items, bool performLock)
		{
			if (!performLock)
				return items.ToImmutableList();

			lock (items)
				return items.ToImmutableList();
		}

		public static void SortByOrdinal<TElement>(this List<TElement> items)
		{
			items.Sort((left, right) =>
			{
				var leftOrdinal = left is Type lt ? lt.GetCustomAttribute<OrdinalAttribute>() : left?.GetType().GetCustomAttribute<OrdinalAttribute>();
				var rightOrdinal = right is Type rt ? rt.GetCustomAttribute<OrdinalAttribute>() : right?.GetType().GetCustomAttribute<OrdinalAttribute>();

				if (leftOrdinal is null && rightOrdinal is null)
					return 0;

				if (leftOrdinal is not null && rightOrdinal is null)
					return -1;

				if (leftOrdinal is null && rightOrdinal is not null)
					return 1;

				if (leftOrdinal?.Ordinal == rightOrdinal?.Ordinal)
					return 0;
				else if (leftOrdinal?.Ordinal < rightOrdinal?.Ordinal)
					return 1;
				else
					return -1;
			});
		}

		public static void SortByPriority<TElement>(this List<TElement> items)
		{
			items.Sort((left, right) =>
			{
				var leftPriority = left is Type lt ? lt.GetCustomAttribute<PriorityAttribute>() : left?.GetType().GetCustomAttribute<PriorityAttribute>();
				var rightPriority = right is Type rt ? rt.GetCustomAttribute<PriorityAttribute>() : right?.GetType().GetCustomAttribute<PriorityAttribute>();

				if (leftPriority is null && rightPriority is null)
					return 0;

				if (leftPriority is not null && rightPriority is null)
					return -1;

				if (leftPriority is null && rightPriority is not null)
					return 1;

				if (leftPriority?.Priority == rightPriority?.Priority)
					return 0;
				else if (leftPriority?.Priority > rightPriority?.Priority)
					return -1;
				else
					return 1;
			});
		}
	}
}
