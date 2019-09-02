using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TomPIT.Annotations;
using TomPIT.Data;

namespace TomPIT.Services
{
	public abstract class ProcessHandler : IProcessHandler, IUniqueValueProvider
	{
		protected ProcessHandler()
		{
		}

		protected ProcessHandler(IDataModelContext context)
		{
			Context = context;
		}

		[JsonIgnore]
		[SkipValidation]
		public IDataModelContext Context { get; private set; }

		protected virtual void OnValidating(List<ValidationResult> results)
		{

		}

		protected virtual List<object> OnProvideUniqueValues(string propertyName)
		{
			return null;
		}

		bool IUniqueValueProvider.IsUnique(IDataModelContext context, string propertyName)
		{
			return IsValueUnique(propertyName);
		}

		protected virtual bool IsValueUnique(string propertyName)
		{
			return true;
		}

		public void Validate()
		{
			ValidateRoot();
			Validate(this, true);
		}

		private void ValidateRoot()
		{
			var af = GetType().FindAttribute<ValidateAntiforgeryAttribute>();

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

		protected void Validate(object instance)
		{
			Validate(instance, false);
		}

		private void Validate(object instance, bool triggerValidating)
		{
			var results = new List<ValidationResult>();
			var refs = new List<object>();

			ValidateProperties(results, instance, refs);

			if (results.Count == 0 && triggerValidating)
				OnValidating(results);

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

			var properties = instance.GetType().GetProperties();

			foreach (var property in properties)
			{
				if (property.GetMethod == null || !property.GetMethod.IsPublic)
					continue;

				var skipAtt = property.FindAttribute<SkipValidationAttribute>();

				if (skipAtt != null)
					continue;

				if (property.PropertyType.IsTypePrimitive())
					ValidateProperty(results, instance, property);
				else if (property.PropertyType.IsCollection())
				{
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
					var value = GetValue(instance, property);

					if (value == null)
						continue;

					ValidateProperties(results, value, references);
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

		void IProcessHandler.Initialize(IDataModelContext context)
		{
			Context = context;
		}
	}
}
