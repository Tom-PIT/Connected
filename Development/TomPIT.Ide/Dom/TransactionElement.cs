using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Compilation;
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
				var r = SaveText(property, attribute, value);

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

		private ITransactionResult Execute(object instance, PropertyInfo property, string value)
		{
			return new PropertyWriter(this).Write(instance, property.Name, value);
		}

		private ITransactionResult SaveText(string property, string attribute, string value)
		{
			if (string.Compare(property, "Template", true) == 0 && string.Compare(attribute, "Text", true) == 0)
				return SaveText(value);

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

		private ITransactionResult SaveText(string value)
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

				Connection.GetService<ICompilerService>().Invalidate(Environment.Context, this.MicroService(), se.Configuration().Component(Environment.Context), se);

				r.Invalidate |= EnvironmentSection.Events;
			}

			return r;
		}
	}
}
