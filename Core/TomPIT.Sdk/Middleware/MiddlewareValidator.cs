﻿using Microsoft.AspNetCore.Antiforgery;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using TomPIT.Annotations;
using TomPIT.Data;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Globalization;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Security;

namespace TomPIT.Middleware
{
	internal delegate void ValidatingHandler(object sender, List<ValidationResult> results);
	internal class MiddlewareValidator : MiddlewareObject
	{
		public event ValidatingHandler? Validating;

		private ITraceService _traceService;
		private Guid _validationId = Guid.NewGuid();

		public MiddlewareValidator(IMiddlewareComponent instance)
			  : base(instance.Context)
		{
			Instance = instance;
			_traceService = Context.Tenant.GetService<ITraceService>();
		}

		private IMiddlewareComponent Instance { get; }

		public async Task Validate()
		{
			await ValidateRoot();
			await Validate(Instance, true);
		}

		private async Task ValidateRoot()
		{
			var sw = Stopwatch.StartNew();

			if (Instance.Context is IElevationContext elevation && elevation.State == ElevationContextState.Granted)
				return;

			if (Instance.GetType().FindAttribute<ValidateAntiforgeryAttribute>() is ValidateAntiforgeryAttribute attribute && !attribute.ValidateRequest)
				return;

			if (Shell.HttpContext is null || !Context.Tenant.GetService<IRuntimeService>().Features.HasFlag(InstanceFeatures.Application))
				return;

			if (!Shell.HttpContext.Request.IsAjaxRequest())
				return;

			if (Shell.HttpContext.RequestServices.GetService(typeof(IAntiforgery)) is not IAntiforgery service)
				return;

			try
			{
				Trace($"Validating antiforgery {sw.ElapsedMilliseconds}");
				//if (AsyncUtils.RunSync(() => service.IsRequestValidAsync(Shell.HttpContext)))
				//{
				//    Trace($"Validation exited due to valid antiforgery found after {sw.ElapsedMilliseconds}");
				//    return;
				//}
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				Trace($"Antiforgery request validation failed due to error {ex}");
			}

			//throw new MiddlewareValidationException(Instance, SR.ValAntiForgery);
		}

		public async Task Validate(object instance, bool triggerValidating)
		{
			var results = new List<ValidationResult>();
			var refs = new List<object>();

			ValidateProperties(results, instance, refs);

			if (!results.Any() && triggerValidating)
				TriggerValidating(this, results);

			if (results.Any())
				throw new MiddlewareValidationException(instance, results);

			await Task.CompletedTask;
		}

		private void TriggerValidating(object sender, List<ValidationResult> results)
		{
			Validating?.Invoke(sender, results);
		}

		private void ValidateProperties(List<ValidationResult> results, object instance, List<object> references)
		{
			if (instance is null)
				return;

			if (instance.GetType().IsTypePrimitive())
				return;

			if (instance is null || references.Contains(instance))
				return;

			references.Add(instance);

			var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			if (!properties.Any())
				return;

			var publicProps = new List<PropertyInfo>();
			var nonPublicProps = new List<PropertyInfo>();

			foreach (var property in properties)
			{
				if (property.GetMethod is null)
					continue;

				var skipAtt = property.FindAttribute<SkipValidationAttribute>();

				if (skipAtt is not null)
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
			if (results.Any())
				return;
			/*
			 * Second step is to validate complex public members and collections. 
			 */
			foreach (var property in publicProps)
			{
				if (property.PropertyType.IsCollection())
				{
					if (GetValue(instance, property) is not IEnumerable ien)
						continue;

					var en = ien.GetEnumerator();

					while (en.MoveNext())
					{
						if (en.Current is null)
							continue;

						ValidateProperties(results, en.Current, references);
					}
				}
				else
				{
					var value = GetValue(instance, property);

					if (value is null)
						continue;

					ValidateProperties(results, value, references);
				}
			}
			/*
			 * If any complex validation failed we won't validate private members because
			 * it is possible that initialization would fail for the reason of validation being failed.
			 */
			if (results.Any())
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

			if (property is null)
				return;

			var attributes = property.GetCustomAttributes(false);

			if (!ValidateRequestValue(results, instance, property, proposedValue))
				return;

			if (property.PropertyType.IsEnum && !Enum.TryParse(property.PropertyType, Types.Convert<string>(proposedValue), out _))
				results.Add(new MiddlewareValidationResult(instance, $"{SR.ValEnumValueNotDefined} ({property.PropertyType.ShortName()}, {property.GetValue(instance)})"));

			foreach (var attribute in attributes)
			{
				if (attribute is ValidationAttribute val)
				{
					try
					{
						var serviceProvider = new ValidationServiceProvider();

						serviceProvider.AddService(typeof(IMiddlewareContext), context);
						serviceProvider.AddService(typeof(IUniqueValueProvider), instance);

						var ctx = new ValidationContext(instance, serviceProvider, new Dictionary<object, object?>
										  {
												 { "entity", instance }
										  })
						{
							DisplayName = property.Name,
							MemberName = property.Name
						};

						val.Validate(proposedValue, ctx);
					}
					catch (ValidationException ex)
					{
						results.Add(new MiddlewareValidationResult(instance, ex.Message, new List<string> { property.Name }));
					}
				}
			}
		}

		private void ValidateProperty(List<ValidationResult> results, object instance, PropertyInfo property)
		{
			var attributes = property.GetCustomAttributes(false);

			var localizeAttribute = attributes.FirstOrDefault(e => e is LocalizedDisplayAttribute);

			if (!ValidateRequestValue(results, instance, property))
				return;

			if (property.PropertyType.IsEnum && !Enum.TryParse(property.PropertyType, Types.Convert<string>(property.GetValue(instance)), out _))
				results.Add(new MiddlewareValidationResult(instance, $"{SR.ValEnumValueNotDefined} ({property.PropertyType.ShortName()}, {property.GetValue(instance)})"));

			foreach (var attribute in attributes)
			{
				if (attribute is ValidationAttribute val)
				{
					try
					{
						var serviceProvider = new ValidationServiceProvider();

						serviceProvider.AddService(typeof(IMiddlewareContext), Context);
						serviceProvider.AddService(typeof(IUniqueValueProvider), Instance);

						var displayName = property.Name;

						if (localizeAttribute is LocalizedDisplayAttribute lda && instance is MiddlewareApiOperation operationInstance)
						{
							var localizationService = MiddlewareDescriptor.Current.Tenant.GetService<ILocalizationService>();

							try
							{
								var msTokens = lda.StringTable.Split('/');

								displayName = localizationService?.GetString(msTokens[0], msTokens[1], lda.Key, Thread.CurrentThread.CurrentCulture.LCID, false);
							}
							catch
							{
								displayName = property.Name;
							}
						}

						var ctx = new ValidationContext(instance, serviceProvider, new Dictionary<object, object?>
								{
									  { "entity", this }
								})
						{
							DisplayName = displayName.ToLower(),
							MemberName = property.Name,
						};

						val = LocalizeAttribute(val);

						val.Validate(GetValue(instance, property), ctx);
					}
					catch (ValidationException ex)
					{
						if (ex is MiddlewareValidationException mve)
							throw new MiddlewareValidationException(instance, ex.Message, mve);
						else
							throw new MiddlewareValidationException(instance, ex.Message);
					}
				}
			}
		}

		private static ValidationAttribute LocalizeAttribute(ValidationAttribute attribute)
		{
			var attributeName = (attribute.TypeId as Type)?.Name ?? null;

			if (string.IsNullOrEmpty(attributeName))
				return attribute;

			var property = typeof(SR).GetProperty(attributeName, BindingFlags.Static | BindingFlags.Public);

			if (property is null)
				return attribute;

			var value = property.GetValue(null) as string;

			if (string.IsNullOrWhiteSpace(value))
				return attribute;

			attribute.ErrorMessageResourceName = attributeName;
			attribute.ErrorMessageResourceType = typeof(SR);

			return attribute;
		}

		private void Trace(string message)
		{
			if (!Context.Tenant.GetService<IRuntimeService>().Features.HasFlag(InstanceFeatures.Application))
				return;

			_traceService?.Trace(nameof(MiddlewareValidator), "RootValidation", $"[{DateTimeOffset.UtcNow}] [{_validationId}] {message}");
		}

		private static bool ValidateRequestValue(List<ValidationResult> results, object instance, PropertyInfo property, object? value)
		{
			if (value is null)
				return true;

			var att = property.FindAttribute<ValidateRequestAttribute>();

			if (att is not null && !att.ValidateRequest)
				return true;

			var decoded = HttpUtility.HtmlDecode(value.ToString());

			if (decoded is not null && decoded.Replace(" ", string.Empty).Contains("<script>"))
			{
				results.Add(new MiddlewareValidationResult(instance, SR.ValScriptTagNotAllowed));
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

		private static object? GetValue(object component, PropertyInfo property)
		{
			try
			{
				return property.GetValue(component);
			}
			catch (TargetInvocationException tex)
			{
				if (tex.InnerException is ValidationException)
					throw tex.InnerException;

				throw TomPITException.Unwrap(component, tex);
			}
			catch (ValidationException)
			{
				throw;
			}
		}
	}
}
