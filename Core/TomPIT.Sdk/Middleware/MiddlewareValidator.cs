using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

			if (Shell.HttpContext == null || !Context.Environment.IsInteractive)
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
			if (instance == null)
				return;

			if (instance.GetType().IsTypePrimitive())
				return;

			if (instance == null || references.Contains(instance))
				return;

			references.Add(instance);

			var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			if (properties.Length == 0)
				return;

			var publicProps = new List<PropertyInfo>();
			var nonPublicProps = new List<PropertyInfo>();

			foreach (var property in properties)
			{
				if (property.GetMethod == null)
					continue;

				var skipAtt = property.FindAttribute<SkipValidationAttribute>();

				if (skipAtt != null)
					continue;

				if (property.GetMethod.IsPublic)
					publicProps.Add(property);
				else
					nonPublicProps.Add(property);
			}
			/*
			 * First, iterate only through the public properties
			 * At this point we won't validate complex objects, only the attributes directly on the
			 * passed instance
			 */
			foreach (var property in publicProps)
				ValidateProperty(results, instance, property);
			/*
			 * If root validation failed we won't go deep because this would probably cause
			 * duplicate and/or confusing validation messages
			 */
			if (results.Count > 0)
				return;
			/*
			 * Second step is to validate complex public members and collections. 
			 */
			foreach (var property in publicProps)
			{
				if (property.PropertyType.IsCollection())
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
			/*
			 * If any complex validation failed we won't validate private members because
			 * it is possible that initialization would fail for the reason of validation being failed.
			 */
			if (results.Count > 0)
				return;
			/*
			 * Now that validation of the public properties succeed we can go validate nonpublic members
			 */
			foreach (var property in nonPublicProps)
				ValidateProperty(results, instance, property);
		}

		public static void ValidatePropertyValue(IMiddlewareContext context, List<ValidationResult> results, object instance, string propertyName, object proposedValue)
		{
			var property = instance.GetType().GetProperty(propertyName);

			if (property == null)
				return;

			var attributes = property.GetCustomAttributes(false);

			if (!ValidateRequestValue(results, instance, property, proposedValue))
				return;

			if (property.PropertyType.IsEnum && !property.PropertyType.IsEnumDefined(property.GetValue(instance)))
				results.Add(new ValidationResult($"{SR.ValEnumValueNotDefined} ({property.PropertyType.ShortName()}, {property.GetValue(instance)})"));

			foreach (var attribute in attributes)
			{
				if (attribute is ValidationAttribute val)
				{
					try
					{
						var serviceProvider = new ValidationServiceProvider();

						serviceProvider.AddService(typeof(IMiddlewareContext), context);
						serviceProvider.AddService(typeof(IUniqueValueProvider), instance);

						var ctx = new ValidationContext(instance, serviceProvider, new Dictionary<object, object>
						{
							{ "entity", instance }
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

		private void ValidateProperty(List<ValidationResult> results, object instance, PropertyInfo property)
		{
			var attributes = property.GetCustomAttributes(false);

			if (!ValidateRequestValue(results, instance, property))
				return;

			if (property.PropertyType.IsEnum && !property.PropertyType.IsEnumDefined(property.GetValue(instance)))
				results.Add(new ValidationResult($"{SR.ValEnumValueNotDefined} ({property.PropertyType.ShortName()}, {property.GetValue(instance)})"));

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

		private static bool ValidateRequestValue(List<ValidationResult> results, object instance, PropertyInfo property, object value)
		{
			if (value == null)
				return true;

			var att = property.FindAttribute<ValidateRequestAttribute>();

			if (att != null && !att.ValidateRequest)
				return true;

			var decoded = HttpUtility.HtmlDecode(value.ToString());

			if (decoded.Replace(" ", string.Empty).Contains("<script>"))
			{
				results.Add(new ValidationResult(SR.ValScriptTagNotAllowed));
				return false;
			}

			return true;
		}
		private static bool ValidateRequestValue(List<ValidationResult> results, object instance, PropertyInfo property)
		{
			if (property.PropertyType != typeof(string))
				return true;

			if (!property.CanWrite)
				return true;

			return ValidateRequestValue(results, instance, property, GetValue(instance, property));
		}

		private static object GetValue(object component, PropertyInfo property)
		{
			try
			{
				return property.GetValue(component);
			}
			catch (TargetInvocationException tex)
			{
				if (tex.InnerException is ValidationException)
					throw tex.InnerException;

				return null;
			}
			catch (ValidationException vex)
			{
				throw vex;
			}
			catch
			{
				return null;
			}
		}
	}
}
