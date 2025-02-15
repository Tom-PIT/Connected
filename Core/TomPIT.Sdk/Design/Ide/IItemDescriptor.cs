﻿using System;

namespace TomPIT.Design.Ide
{
	public interface IItemDescriptor
	{
		string Text { get; }
		string Id { get; }
		Type Type { get; }
		object Value { get; }
		string Glyph { get; }
		string Category { get; }
		int Ordinal { get; }
	}
}
