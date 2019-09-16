using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class ManifestAttribute : Attribute
	{
		public ManifestAttribute() { }

		public ManifestAttribute(string type)
		{
			TypeName = type;
		}
		public ManifestAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}