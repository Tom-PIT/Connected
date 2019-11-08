using System;
using System.Collections.Generic;

namespace TomPIT.Navigation
{
	public interface IRoute
	{
		string Text { get; }
		string Url { get; }
		bool Enabled { get; }
		int Ordinal { get; }
		string Glyph { get; }
		string Css { get; }
		bool IsActive { get; }
		bool BeginGroup { get; }
		string Id { get; set; }
		List<IRoute> Items { get; }
		Guid Token { get; }
		string Target { get; }
		bool Visible { get; }
		string Category { get; }
	}
}