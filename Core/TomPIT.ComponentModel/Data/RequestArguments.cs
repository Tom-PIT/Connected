using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace TomPIT.Data
{
	public abstract class RequestArguments:RequestEntity
	{
		public void Validate()
		{
			var results = new List<ValidationResult>();

			ValidateProperties(results, this);

			var sb = new StringBuilder();

			foreach (var result in results)
			{
				if (result != null)
					sb.AppendLine(result.ErrorMessage);
			}

			if (sb.Length > 0)
				throw new RuntimeException(sb.ToString());
		}

		private void ValidateProperties(List<ValidationResult> results, object instance)
		{
			if (instance == null)
				return;

			if (instance is RequestEntity re)
				re.OnValidating(results);

			var properties = instance.GetType().GetProperties();

			foreach (var property in properties)
			{
				if (property.GetType().IsCollection())
				{
					var ien = property.GetValue(instance) as IEnumerable;
					var en = ien.GetEnumerator();

					while (en.MoveNext())
					{
						if (en.Current == null)
							continue;

						ValidateProperties(results, en.Current);
					}
				}
				else if (property.GetType().IsTypePrimitive())
					ValidateProperty(results, instance, property);
				else
				{
					var value = property.GetValue(instance);

					if (value == null)
						continue;

					ValidateProperties(results, value);
				}
			}
		}

		private void ValidateProperty(List<ValidationResult> results, object instance, PropertyInfo property)
		{
			var attributes = property.GetCustomAttributes(false);

			foreach (var attribute in attributes)
			{
				if (attribute is ValidationAttribute val)
				{
					try
					{
						val.Validate(property.GetValue(instance), property.Name);
					}
					catch (ValidationException ex)
					{
						results.Add(new ValidationResult(ex.Message, new List<string> { property.Name }));
					}
				}
			}
		}
	}
}
