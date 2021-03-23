using System;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Properties;
using TomPIT.Design.Ide.Validation;
using TomPIT.Ide.Environment;
using TomPIT.Ide.Properties.Validation;

namespace TomPIT.Ide.Properties
{
	internal class Property : EnvironmentObject, IProperty
	{
		private PropertyValidation _val = null;

		public Property(IEnvironment environment, IDomElement element, PropertyInfo property) : base(environment)
		{
			Element = element;
			PropertyInfo = property;
		}

		public IDomElement Element { get; private set; }
		public string Name { get; set; }
		public int Ordinal { get; set; }
		public string Category { get; set; }
		public object Value { get; set; }
		public string Text { get; set; }
		public string Editor { get; set; }
		public Type Type { get; set; }
		public bool IsLocalizable { get; set; }
		public bool SupportsTimezone { get; set; }
		public string HelpLink { get; set; }
		public string Description { get; set; }
		public string Format { get; set; }
		public bool Obsolete { get; set; }
		public IConfigurationElement ContextElement { get; set; }

		public PropertyInfo PropertyInfo { get; set; }

		public IPropertyValidation Validation
		{
			get
			{
				if (_val == null)
					_val = new PropertyValidation();

				return _val;
			}
		}
	}
}
