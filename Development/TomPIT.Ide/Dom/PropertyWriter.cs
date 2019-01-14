using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class PropertyWriter
	{
		public PropertyWriter(IEnvironment environment)
		{
			Environment = environment;
		}

		private IEnvironment Environment { get; }

		public ITransactionResult Write(object instance, string propertyName, string value)
		{
			var property = instance.GetType().GetProperty(propertyName);

			if (property == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrPropertyNotFound, propertyName));

			if (!Types.TryConvert(value, out object validatedValue, property.PropertyType))
				throw IdeException.ConversionError(this, IdeEvents.SaveProperty, property.Name, value, property.PropertyType);

			var aw = property.FindAttribute<AllowWhitespaceAttribute>();

			if (aw != null && value is string && string.IsNullOrWhiteSpace(validatedValue as string))
				validatedValue = "&nbsp;";

			ValidateValue(instance, property, validatedValue);

			if (!SaveLocalizedValue(instance, property, validatedValue))
				SaveNonLocalizedValue(instance, property, validatedValue);

			var r = new TransactionResult(true);
			var att = property.FindAttribute<InvalidateEnvironmentAttribute>();

			if (att != null)
				r.Invalidate = att.Sections;

			r.Component = instance;

			if (instance.GetType().ImplementsInterface<ISourceCode>())
			{
				var se = instance as ISourceCode;

				Environment.Context.Connection().GetService<ICompilerService>().Invalidate(Environment.Context, Environment.Context.MicroService(), se.Configuration().Component(Environment.Context), se);

				r.Invalidate |= EnvironmentSection.Events;
			}

			return r;
		}

		private void SaveNonLocalizedValue(object instance, PropertyInfo property, object value)
		{
			var element = instance as IElement;

			var att = property.FindAttribute<HtmlTextAttribute>();

			if (att != null)
				value = att.Sanitize(Environment.Context, element, property, property.GetValue(instance), value);

			property.SetValue(instance, value);
		}

		private bool SaveLocalizedValue(object instance, PropertyInfo property, object value)
		{
			var element = instance as IElement;

			if (element == null)
				return false;

			if (Environment.Globalization.LanguageToken == Guid.Empty)
				return false;

			var loc = property.FindAttribute<LocalizableAttribute>();

			if (loc == null || !loc.IsLocalizable)
				return false;

			var att = new HtmlTextAttribute();

			var sanitized = att.Sanitize(Environment.Context, element, property, property.GetValue(instance), value);
			var text = Types.Convert<string>(sanitized);

			Environment.Context.Connection().GetService<IMicroServiceDevelopmentService>().UpdateString(Environment.Context.MicroService(), Environment.Globalization.LanguageToken, element.Id, property.Name, text);

			return true;
		}

		protected void ValidateValue(object instance, PropertyInfo pi, object value)
		{
			var ctx = new ValidationContext(instance)
			{
				MemberName = pi.Name
			};

			var results = new List<ValidationResult>();

			if (!Validator.TryValidateProperty(value, ctx, results))
			{
				var sb = new StringBuilder();

				foreach (var i in results)
					sb.AppendFormat("{0} ", i.ErrorMessage);

				throw IdeException.ValidationFailed(this, IdeEvents.SaveProperty, sb.ToString());
			}
		}
	}
}
