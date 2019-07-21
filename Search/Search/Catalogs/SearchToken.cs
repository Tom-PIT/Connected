using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TomPIT.Search.Catalogs
{
	internal class SearchToken
	{
		public string Key { get; set; }
		public string Text { get; set; }
		public string Format { get; set; }
		public string StringTable { get; set; }

		public SearchToken() { }
		public SearchToken(string stringTable, string key, string text, string format)
		{
			StringTable = stringTable;
			Key = key;
			Text = text;
			Format = format;
		}

		public void Deserialize(string value)
		{
			Key = string.Empty;
			Text = string.Empty;
			Format = string.Empty;
			StringTable = string.Empty;

			if (string.IsNullOrWhiteSpace(value))
				return;

			if (!value.StartsWith("[base16") || !value.EndsWith("]"))
				return;

			try
			{
				var content = value.Substring(7, value.Length - 8);
				var raw = content.FromBase16();
				var tokens = raw.Split(',');

				if (tokens == null)
					return;

				foreach (var i in tokens)
				{
					var subTokens = i.Split(":".ToCharArray(), 2);

					if (subTokens.Length != 2)
						continue;

					var argument = subTokens[0];

					if (string.Compare(argument, "k", true) == 0)
					{
						var stringTableQualifier = subTokens[1].Split("/".ToCharArray());

						Key = stringTableQualifier[stringTableQualifier.Length - 1];
						var table = new StringBuilder();

						for (var j = 0; j < stringTableQualifier.Length - 2; j++)
							table.Append($"{stringTableQualifier[j]}/");

						StringTable = table.ToString().TrimEnd('/');
					}
					else if (string.Compare(argument, "f", true) == 0)
						Format = subTokens[1];
				}

			}
			catch { }
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(Key))
				sb.AppendFormat("k:{0},", Key);

			if (!string.IsNullOrWhiteSpace(Format))
				sb.AppendFormat("f:{0},", Format);

			return string.Format("[base16{0}]: [{1}]", sb.ToString().TrimEnd(',').ToBase16(), Text);
		}
	}
}