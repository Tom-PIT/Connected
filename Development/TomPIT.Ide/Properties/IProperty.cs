using System;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;
using TomPIT.Ide.Properties.Validation;

namespace TomPIT.Ide.Properties
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

		IConfigurationElement ContextElement { get; }

		PropertyInfo PropertyInfo { get; }
	}
}
