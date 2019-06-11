using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Services;

namespace TomPIT.Data
{
	public abstract class RequestArguments : RequestEntity
	{

		protected RequestArguments (IDataModelContext context)
		{
			Context = context;
		}

		protected IDataModelContext Context { get; }
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

			var properties = instance.GetType().GetProperties();

			foreach (var property in properties)
			{
				if (property.PropertyType.IsTypePrimitive())
					ValidateProperty(results, instance, property);
				else if (property.PropertyType.IsCollection())
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
				else
				{
					var value = property.GetValue(instance);

					if (value == null)
						continue;

					ValidateProperties(results, value);
				}
			}

			if (results.Count == 0)
			{
				if (instance is RequestEntity re)
					re.Validate(Context, results);
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
						var serviceProvider = new ValidationServiceProvider();

						serviceProvider.AddService(typeof(IDataModelContext), Context);
						serviceProvider.AddService(typeof(IUniqueValueProvider), this);

						var ctx = new ValidationContext(instance, serviceProvider, new Dictionary<object, object>
						{
							{ "entity", this }
						})
						{
							DisplayName = property.Name,
							MemberName = property.Name
						};

						val.Validate(property.GetValue(instance), ctx);
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