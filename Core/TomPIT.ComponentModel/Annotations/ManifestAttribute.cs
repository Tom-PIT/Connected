using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
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