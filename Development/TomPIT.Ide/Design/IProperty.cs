using System;
using System.Reflection;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Validation;

namespace TomPIT.Design
{
	public interface IProperty : IEnvironmentClient, IDomClient
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

		PropertyInfo PropertyInfo { get; }
	}
}
