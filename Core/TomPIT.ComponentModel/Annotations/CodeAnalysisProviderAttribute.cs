using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class CodeAnalysisProviderAttribute : Attribute
	{
		public CodeAnalysisProviderAttribute() { }

		public CodeAnalysisProviderAttribute(string type)
		{
			TypeName = type;
		}
		public CodeAnalysisProviderAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}