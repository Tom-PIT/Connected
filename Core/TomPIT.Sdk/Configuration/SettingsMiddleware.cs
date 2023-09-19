using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
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

		[Browsable(false)]
		public string NameSpace { get; set; }

		protected T GetValue<T>([CallerMemberName] string name = null)
		{
			var result = Context.Tenant.GetService<ISettingService>().GetValue<object>(name, NameSpace, Type, PrimaryKey);

			if (result == null)
			{
				var property = GetType().GetProperty(name);
				var defaultValue = property.FindAttribute<DefaultValueAttribute>();

				if (defaultValue != null)
					return Types.Convert<T>(defaultValue.Value);
				else
					return default;
			}

			return Types.Convert<T>(result);
		}

		protected void SetValue<T>(string name, T value)
		{
			Validate(name, value);
			Context.Tenant.GetService<ISettingService>().Update(name, NameSpace, Type, PrimaryKey, value);
		}

		protected virtual void SetProp<T>(T value, [CallerMemberName] string name = null) => SetValue(name, value);

		protected virtual T GetProp<T>([CallerMemberName] string name = null) => GetValue<T>(name);

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
