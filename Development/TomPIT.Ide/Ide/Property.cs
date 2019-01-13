using System;
using System.Reflection;
using TomPIT.Design;
using TomPIT.Design.Validation;
using TomPIT.Dom;
using TomPIT.Validation;

namespace TomPIT.Ide
{
	internal class Property : EnvironmentClient, IProperty
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
