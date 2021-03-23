using System;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide;

namespace TomPIT.Ide.Collections
{
	public class ItemDescriptor : IItemDescriptor
	{
		public ItemDescriptor()
		{

		}

		public ItemDescriptor(string text, object value) : this(text, null, null)
		{
			Value = value;
		}

		public ItemDescriptor(string text, string id, Type type)
		{
			Text = text;
			Id = id;
			Type = type;
		}

		public string Text { get; set; }
		public string Glyph { get; set; }
		public string Id { get; set; }
		public string Category { get; set; }
		public Type Type { get; set; }
		public int Ordinal { get; set; }
		public object Value { get; set; }

		public static IItemDescriptor From(IComponent d)
		{
			if (d == null)
				return null;

			var r = new ItemDescriptor
			{
				Text = d.Name,
				Type = Reflection.TypeExtensions.GetType(d.Type),
				Id = d.Token.ToString()
			};

			return r;
		}
	}
}
