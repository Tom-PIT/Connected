using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public sealed class DefinitionProviderAttribute : Attribute
	{
		public const string DistributedEventProvider = "TomPIT.Development.TextEditor.CSharp.Services.DefinitionProviders.DistributedEventProvider, " + SystemAssemblies.DevelopmentAssembly;

		public DefinitionProviderAttribute() { }

		public DefinitionProviderAttribute(string type)
		{
			TypeName = type;
		}
		public DefinitionProviderAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}
