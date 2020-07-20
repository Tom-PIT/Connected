using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Configuration
{
	public abstract class SettingsMiddleware : MiddlewareComponent, ISettingsMiddleware
	{
		protected T GetValue<T>(string name)
		{
			return GetValue<T>(name, null, null);
		}
		protected T GetValue<T>(string name, string type, string primaryKey)
		{
			return Context.Tenant.GetService<ISettingService>().GetValue<T>(name, type, primaryKey);
		}

		protected void SetValue<T>(string name, T value)
		{
			SetValue<T>(name, null, null, value);
		}
		protected void SetValue<T>(string name, string type, string primaryKey, T value)
		{
			Validate(name, value);

			Context.Tenant.GetService<ISettingService>().Update(name, type, primaryKey, value);
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
