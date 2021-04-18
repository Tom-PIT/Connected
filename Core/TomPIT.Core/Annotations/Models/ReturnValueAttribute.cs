using System;

namespace TomPIT.Annotations.Models
{
	public enum PropertyValueBehavior
	{
		OverwriteDefault = 1,
		AlwaysOverwrite = 2,
	}
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ReturnValueAttribute : Attribute
	{
		public PropertyValueBehavior ValueBehavior { get; set; } = PropertyValueBehavior.OverwriteDefault;
	}
}
