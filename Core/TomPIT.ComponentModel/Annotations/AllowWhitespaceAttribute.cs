using System;

namespace TomPIT.Annotations
{
	/// <summary>
	/// This attribute represents automatic value injection for
	/// properties of the string type.If the property value at
	/// the point of saving is empty it is automatically replaced
	/// with <code>&nbsp;</code> html value.
	/// </summary>
	public class AllowWhitespaceAttribute : Attribute
	{
		public AllowWhitespaceAttribute()
		{ }
	}
}