using System;

namespace TomPIT.Annotations
{
	public enum DateEditorFormat
	{
		Date = 1,
		DateTime = 2,
		Time = 3
	}
	[AttributeUsage(AttributeTargets.Property)]
	public class DateEditorFormatAttribute : Attribute
	{
		public DateEditorFormatAttribute(DateEditorFormat format)
		{
			Format = format;
		}

		public DateEditorFormat Format { get; }
	}
}
