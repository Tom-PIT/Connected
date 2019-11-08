using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Property)]
	public class PropertyEditorAttribute : Attribute
	{
		public const string Select = "Select";
		public const string TextArea = "TextArea";
		public const string Text = "Text";
		public const string Label = "Label";
		public const string Check = "Check";
		public const string Number = "Number";
		public const string Date = "Date";
		public const string Color = "Color";
		public const string Tag = "Tag";

		public PropertyEditorAttribute() { }

		public PropertyEditorAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}