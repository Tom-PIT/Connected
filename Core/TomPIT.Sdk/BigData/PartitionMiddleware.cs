﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations.BigData;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.BigData
{
	public abstract class PartitionMiddleware<T> : MiddlewareComponent, IPartitionMiddleware<T>
	{
		public TimestampBehavior Timestamp { get; protected set; } = TimestampBehavior.Static;

		public List<T> Invoke(List<T> items)
		{
			Validate(items);

			return OnInvoke(items);
		}

		protected virtual List<T> OnInvoke(List<T> items)
		{
			return items;
		}

		private void Validate(List<T> items)
		{
			Validate();

			if (items == null)
				return;

			foreach (var item in items)
				ValidateItem(item);
		}

		private void ValidateItem(T item)
		{
			Validate(item);

			var properties = item.GetType().GetProperties();

			var key = false;
			var partitionKey = false;

			foreach (var property in properties)
			{
				if (!property.CanRead || property.GetMethod.IsPublic)
					continue;

				var keyAtt = property.FindAttribute<BigDataKeyAttribute>();

				if (keyAtt != null)
				{
					if (key)
						throw new ValidationException(SR.ValBigDataMultipleKey);

					key = true;
				}

				var partitionKeyAtt = property.FindAttribute<BigDataPartitionKeyAttribute>();

				if (partitionKeyAtt != null)
				{
					if (partitionKey)
						throw new ValidationException(SR.ValBigDataMultiplePartitionKey);

					key = true;
				}
			}
		}
	}
}