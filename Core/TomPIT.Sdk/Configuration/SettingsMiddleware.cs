﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Configuration
{
	public abstract class SettingsMiddleware : MiddlewareComponent, ISettingsMiddleware
	{
		[Browsable(false)]
		public string Type { get; set; }
		[Browsable(false)]
		public string PrimaryKey { get; set; }

		protected T GetValue<T>(string name)
		{
			var result = Context.Tenant.GetService<ISettingService>().GetValue<object>(name, Type, PrimaryKey);

			if (result == null)
			{
				var property = GetType().GetProperty(name);
				var defaultValue = property.FindAttribute<DefaultValueAttribute>();

				if (defaultValue != null)
					return Types.Convert<T>(defaultValue);
				else
					return default;
			}

			return Types.Convert<T>(result);
		}

		protected void SetValue<T>(string name, T value)
		{
			Validate(name, value);
			Context.Tenant.GetService<ISettingService>().Update(name, Type, PrimaryKey, value);
		}

		protected void Validate<T>(string name, T value)
		{
			var results = new List<ValidationResult>();

			MiddlewareValidator.ValidatePropertyValue(Context, results, this, name, value);

			if (results.Count > 0)
			{
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
		}
	}
}