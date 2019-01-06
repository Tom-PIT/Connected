using System;

namespace TomPIT.Design
{
	public interface IItemDescriptor
	{
		string Text { get; }
		string Id { get; }
		Type Type { get; }
		object Value { get; }
	}
}
