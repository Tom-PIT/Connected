﻿using System;

namespace TomPIT.Annotations.Design
{
	public class SyntaxAttribute : Attribute
	{
		public const string Razor = "razor";
		public const string CSharp = "csharp";
		public const string Javascript = "javascript";
		public const string Css = "css";
		public const string Less = "less";
		public const string Json = "json";

		public SyntaxAttribute(string syntax)
		{
			Syntax = syntax;
		}

		public string Syntax { get; }
	}
}