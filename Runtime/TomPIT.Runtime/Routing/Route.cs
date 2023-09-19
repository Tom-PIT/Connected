using System;
using System.Collections.Generic;
using TomPIT.Navigation;

namespace TomPIT.Routing
{
	public class Route : IRoute
	{
		private List<IRoute> _items = null;

		public Route()
		{

		}

		public Route(string text, string url)
		{
			Text = text;
			Url = url;
		}
		public string Text { get; set; }
		public string Url { get; set; }
		public bool Enabled { get; set; }
		[Obsolete("Please use Priority instead.")]
		public int Ordinal { get; set; }
		public int Priority { get; set; }
		public string Glyph { get; set; }
		public string Css { get; set; }
		public bool IsActive { get; set; }
		public bool BeginGroup { get; set; }
		public string Id { get; set; }
		public string Category { get; set; }
		public List<IRoute> Items
		{
			get
			{
				if (_items == null)
					_items = new List<IRoute>();

				return _items;
			}
		}

		public Guid Token { get; set; }
		public string Target { get; set; }
		public bool Visible { get; set; }
	}
}
