using System;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Validation;

namespace TomPIT.Design.Ide.Properties
{
	public interface IProperty : IEnvironmentObject, IDomObject
	{
		string Category { get; }
		IPropertyValidation Validation { get; }
		Type Type { get; }
		string Editor { get; }
		string Name { get; }
		object Value { get; }
		string HelpLink { get; }
		string Text { get; }
		int Ordinal { get; }
		bool IsLocalizable { get; }
		string Description { get; }
		string Format { get; }
		bool Obsolete { get; }
		bool SupportsTimezone { get; }
		bool IsReadOnly { get; }

		IConfigurationElement ContextElement { get; }

		PropertyInfo PropertyInfo { get; }
	}
}
