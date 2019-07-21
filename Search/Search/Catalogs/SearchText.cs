using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Search.Catalogs
{
	internal class SearchText
	{
		private List<SearchToken> _elements = null;

		public void Add(string stringTable, string key, double value, string format)
		{
			Elements.Add(new SearchToken(stringTable, key, value.ToString(CultureInfo.InvariantCulture), format));
		}

		public void Add(string stringTable, string key, string text, string format)
		{
			if (string.IsNullOrWhiteSpace(text))
				return;

			Elements.Add(new SearchToken(stringTable, key, text, format));
		}

		public void Add(string stringTable, string key, string text)
		{
			Add(stringTable, key, text, null);
		}

		public void Add(string stringTable, string key, DateTime value, string format)
		{
			if (value == DateTime.MinValue)
				return;

			Elements.Add(new SearchToken(stringTable, key, value.ToString(CultureInfo.InvariantCulture), format));
		}

		private List<SearchToken> Elements
		{
			get
			{
				if (_elements == null)
					_elements = new List<SearchToken>();

				return _elements;
			}
		}

		public static implicit operator string(SearchText text)
		{
			return text.ToString();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			foreach (var i in Elements)
				sb.AppendFormat("{0}, ", i.ToString());

			return sb.ToString().TrimEnd(new char[] { ' ', ',' });
		}
	}
}
