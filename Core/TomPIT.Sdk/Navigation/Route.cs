using System;
using System.Collections.Generic;

namespace TomPIT.Navigation
{
	internal class Route : IRoute
	{
		private List<IRoute> _items = null;
		public string Text { get; set; }
		public string Url { get; set; }
		public bool Enabled { get; set; }
		public int Ordinal { get; set; }
		public string Glyph { get; set; }
		public string Css { get; set; }
		public bool IsActive { get; set; }
		public bool BeginGroup { get; set; }
		public string Id { get; set; }
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
		public string Category { get; set; }
	}
}