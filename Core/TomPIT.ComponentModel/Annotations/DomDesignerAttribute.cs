using System;
using TomPIT.Services;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
	public class DomDesignerAttribute : Attribute
	{
		public const string PermissionsDesigner = "TomPIT.Designers.PermissionsDesigner, TomPIT.Design";
		public const string EmptyDesigner = "TomPIT.Designers.EmptyDesigner, TomPIT.Ide";

		public DomDesignerAttribute() { }

		public DomDesignerAttribute(string type)
		{
			TypeName = type;
		}
		public DomDesignerAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }

		public EnvironmentMode Mode { get; set; } = EnvironmentMode.Design;
	}
}