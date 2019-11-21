using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using TomPIT.Annotations;
using TomPIT.Data;
using TomPIT.Reflection;

namespace TomPIT.Middleware
{
	internal delegate void ValidatingHandler(object sender, List<ValidationResult> results);
	internal class MiddlewareValidator : MiddlewareObject
	{
		public event ValidatingHandler Validating;
		public MiddlewareValidator(IMiddlewareComponent instance)
		{
			Instance = instance;
		}

		private IMiddlewareComponent Instance { get; }

		public void Validate()
		{
			ValidateRoot();
			Validate(Instance, true);
		}

		private void ValidateRoot()
		{
			var af = Instance.GetType().FindAttribute<ValidateAntiforgeryAttribute>();

			if (af == null)
				return;

			if (Shell.HttpContext == null)
				return;

			if (!(Shell.HttpContext.RequestServices.GetService(typeof(IAntiforgery)) is IAntiforgery service))
				return;

			if (Task.Run(async () =>
			{
				return await service.IsRequestValidAsync(Shell.HttpContext);
			}).Result)
				return;

			throw new ValidationException(SR.ValAntiForgery)
			{
				Source = GetType().ScriptTypeName()
			};
		}

		public void Validate(object instance, bool triggerValidating)
		{
			var results = new List<ValidationResult>();
			var refs = new List<object>();

			ValidateProperties(results, instance, refs);

			if (results.Count == 0 && triggerValidating)
				Validating?.Invoke(this, results);

			var sb = new StringBuilder();

			foreach (var result in results)
			{
				if (result != null)
					sb.AppendLine(result.ErrorMessage);
			}

			if (sb.Length > 0)
			{
				throw new ValidationException(sb.ToString())
				{
					Source = GetType().ScriptTypeName()
				};
			}

		}
		private void ValidateProperties(List<ValidationResult> results, object instance, List<object> references)
		{
			if (instance == null || references.Contains(instance))
				return;

			references.Add(instance);

			var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var property in properties)
			{
				if (property.GetMethod == null)
					continue;

				var skipAtt = property.FindAttribute<SkipValidationAttribute>();

				if (skipAtt != null)
					continue;

				ValidateProperty(results, instance, property);

				if (!property.GetMethod.IsPublic)
					continue;

				if (property.PropertyType.IsCollection())
				{
					if (!property.GetMethod.IsPublic)
						continue;

					if (!(GetValue(instance, property) is IEnumerable ien))
						continue;

					var en = ien.GetEnumerator();

					while (en.MoveNext())
					{
						if (en.Current == null)
							continue;

						ValidateProperties(results, en.Current, references);
					}
				}
				else
				{
					if (!property.GetMethod.IsPublic)
						continue;

					var value = GetValue(instance, property);

					if (value == null)
						continue;

					ValidateProperties(results, value, references);
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
						var serviceProvider = new ValidationServiceProvider();

						serviceProvider.AddService(typeof(IMiddlewareContext), Context);
						serviceProvider.AddService(typeof(IUniqueValueProvider), Instance);

						var ctx = new ValidationContext(instance, serviceProvider, new Dictionary<object, object>
						{
							{ "entity", this }
						})
						{
							DisplayName = property.Name,
							MemberName = property.Name
						};

						val.Validate(GetValue(instance, property), ctx);
					}
					catch (ValidationException ex)
					{
						results.Add(new ValidationResult(ex.Message, new List<string> { property.Name }));
					}
				}
			}
		}

		private object GetValue(object component, PropertyInfo property)
		{
			try
			{
				return property.GetValue(component);
			}
			catch
			{
				return null;
			}
		}
	}
}
