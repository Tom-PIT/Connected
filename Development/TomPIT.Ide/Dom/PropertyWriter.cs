using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using TomPIT.Annotations.Design;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Exceptions;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Reflection;

namespace TomPIT.Ide.Dom
{
	public class PropertyWriter
	{
		public PropertyWriter(IDomElement element)
		{
			Element = element;
		}

		private IDomElement Element { get; }

		public ITransactionResult Write(object instance, string propertyName, string value)
		{
			var property = instance.GetType().GetProperty(propertyName);

			if (property == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrPropertyNotFound, propertyName));

			if (!Types.TryConvert(value, out object validatedValue, property.PropertyType))
				throw IdeException.ConversionError(this, IdeEvents.SaveProperty, property.Name, value, property.PropertyType);

			ValidateValue(instance, property, validatedValue);

			if (!SaveLocalizedValue(instance, property, validatedValue))
				SaveNonLocalizedValue(instance, property, validatedValue);

			var r = new TransactionResult(true);
			var att = property.FindAttribute<InvalidateEnvironmentAttribute>();

			if (att != null)
				r.Invalidate = att.Sections;

			r.Component = instance;

			if (instance.GetType().ImplementsInterface<IText>())
			{
				var se = instance as IText;

				Element.Environment.Context.Tenant.GetService<ICompilerService>().Invalidate(Element.Environment.Context, Element.MicroService(), se.Configuration().Component, se);

				r.Invalidate |= EnvironmentSection.Events;
			}

			return r;
		}

		private void SaveNonLocalizedValue(object instance, PropertyInfo property, object value)
		{
			property.SetValue(instance, value);
		}

		private bool SaveLocalizedValue(object instance, PropertyInfo property, object value)
		{
			var element = instance as IElement;

			if (element == null)
				return false;

			if (Element.Environment.Globalization.LanguageToken == Guid.Empty)
				return false;

			var loc = property.FindAttribute<LocalizableAttribute>();

			if (loc == null || !loc.IsLocalizable)
				return false;

			Element.Environment.Context.Tenant.GetService<IMicroServiceDevelopmentService>().UpdateString(Element.MicroService(), Element.Environment.Globalization.LanguageToken, element.Id, property.Name, property.GetValue(instance) as string);

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
