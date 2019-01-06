using System;

namespace TomPIT.Annotations
{
	public class SyntaxAttribute : Attribute
	{
		public SyntaxAttribute(string syntax)
		{
			Syntax = syntax;
		}

		public string Syntax { get; }
	}
}
