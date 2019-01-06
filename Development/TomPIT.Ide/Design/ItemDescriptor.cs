using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design
{
	public class ItemDescriptor : IItemDescriptor
	{
		public ItemDescriptor()
		{

		}

		public ItemDescriptor(string text, string id, Type type)
		{
			Text = text;
			Id = id;
			Type = type;
		}

		public ItemDescriptor(string text, object value)
		{
			Text = text;
			Value = value;
		}

		public string Text
		{
			get; set;
		}

		public string Id
		{
			get; set;
		}

		public Type Type
		{
			get; set;
		}

		public object Value { get; set; }

		public static IItemDescriptor From(IComponent d)
		{
			if (d == null)
				return null;

			var r = new ItemDescriptor
			{
				Text = d.Name,
				Type = Types.GetType(d.Type),
				Id = d.Token.ToString()
			};

			return r;
		}
	}
}
