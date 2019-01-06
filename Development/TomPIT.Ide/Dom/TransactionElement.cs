using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.Compilers;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class TransactionElement : Element, ITransactionHandler
	{
		public TransactionElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
		}

		public override ITransactionHandler Transaction => this;

		public IDomElement Element => this;

		public virtual bool Commit(object component, string property, string attribute)
		{
			return false;
		}

		public virtual ITransactionResult Execute(string property, string attribute, string value)
		{
			var pi = DomQuery.Property(this, property, attribute, out object instance);

			if (pi != null)
				return Execute(instance, pi, value);
			else
			{
				var r = SaveTemplate(property, attribute, value);

				if (r != null)
					return r;
			}

			throw IdeException.PropertyNotFound(this, IdeEvents.SaveProperty, this, property);
		}

		protected ITransactionResult Execute(IPropertySource source, string property, string attribute, string value)
		{
			var pi = DomQuery.Property(this, property, attribute, out object instance);

			if (pi != null)
				return Execute(instance, pi, value);

			throw IdeException.PropertyNotFound(this, IdeEvents.SaveProperty, source, property);
		}

		private TransactionResult Execute(object instance, PropertyInfo property, string value)
		{
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

				Connection.GetService<ICompilerService>().Invalidate(Environment.Context, Environment.Context.MicroService(), se.Configuration().Component(Environment.Context), se);

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

			Connection.GetService<IMicroServiceDevelopmentService>().UpdateString(Environment.Context.MicroService(), Environment.Globalization.LanguageToken, element.Id, property.Name, text);

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

		private ITransactionResult SaveTemplate(string property, string attribute, string value)
		{
			if (string.Compare(property, "Template", true) == 0 && string.Compare(attribute, "Text", true) == 0)
				return SaveTemplate(value);

			if (Element is IPropertySource s)
			{
				foreach (var i in s.PropertySources)
				{
					var pi = i.GetType().GetProperty(property);

					if (pi != null && pi.PropertyType.IsText())
						return Save(pi.GetValue(i) as IText, value);
				}
			}

			if (Element.Value != null)
			{
				var pi = Element.Value.GetType().GetProperty(property);

				if (pi != null && pi.PropertyType.IsText())
					return Save(pi.GetValue(Element.Value) as IText, value);
			}

			return Save(Element.Value as IText, value);
		}

		private ITransactionResult SaveTemplate(string value)
		{
			if (Element.Value != null && Element.Value.GetType().IsText())
				return Save(Element.Value as IText, value);

			if (Element is IPropertySource s)
			{
				foreach (var i in s.PropertySources)
				{
					if (i.GetType().IsText())
						return Save(i as IText, value);
				}
			}

			return null;
		}

		private ITransactionResult Save(IText text, string value)
		{
			if (text == null)
				return null;

			Connection.GetService<IComponentDevelopmentService>().Update(text, value);

			var r = new TransactionResult(true)
			{
				Component = Element.Value,
				Invalidate = EnvironmentSection.Explorer
			};

			if (text.GetType().ImplementsInterface<ISourceCode>())
			{
				var se = text as ISourceCode;

				Connection.GetService<ICompilerService>().Invalidate(Environment.Context, Environment.Context.MicroService(), se.Configuration().Component(Environment.Context), se);

				r.Invalidate |= EnvironmentSection.Events;
			}

			return r;
		}
	}
}
