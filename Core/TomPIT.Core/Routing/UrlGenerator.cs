using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomPIT.Routing
{
	public static class UrlGenerator
	{
		private static Dictionary<char, string> _replacements = null;

		internal static Dictionary<char, string> Replacements
		{
			get
			{
				if (_replacements == null)
				{
					_replacements = new Dictionary<char, string>();

					_replacements.Add('$', string.Empty);
					_replacements.Add('%', string.Empty);
					_replacements.Add('#', string.Empty);
					_replacements.Add('@', string.Empty);
					_replacements.Add('!', string.Empty);
					_replacements.Add('*', string.Empty);
					_replacements.Add('?', string.Empty);
					_replacements.Add(';', string.Empty);
					_replacements.Add(':', string.Empty);
					_replacements.Add('~', string.Empty);
					_replacements.Add('+', string.Empty);
					_replacements.Add('=', string.Empty);
					_replacements.Add('`', string.Empty);
					_replacements.Add('(', string.Empty);
					_replacements.Add(')', string.Empty);
					_replacements.Add('[', string.Empty);
					_replacements.Add(']', string.Empty);
					_replacements.Add('{', string.Empty);
					_replacements.Add('}', string.Empty);
					_replacements.Add('|', string.Empty);
					_replacements.Add('\\', string.Empty);
					_replacements.Add('\'', string.Empty);
					_replacements.Add('<', string.Empty);
					_replacements.Add('>', string.Empty);
					_replacements.Add(',', string.Empty);
					_replacements.Add('/', string.Empty);
					_replacements.Add('^', string.Empty);
					_replacements.Add('&', string.Empty);
					_replacements.Add('"', string.Empty);
					_replacements.Add('.', string.Empty);
					_replacements.Add('č', "c");
					_replacements.Add('š', "s");
					_replacements.Add('ž', "z");
					_replacements.Add('đ', "d");
					_replacements.Add('ć', "c");
				}

				return _replacements;
			}
		}

		public static string GenerateUrl(string text)
		{
			return GenerateUrl(string.Empty, text, null);
		}

		internal static string PrepareString(string text)
		{
			if (string.IsNullOrEmpty(text))
				return null;

			var result = text.Trim();

			result = text.Trim().Trim('-').ToLower().Replace(".", "-").Replace(' ', '-');

			var ds = Replacements.Keys.ToList();

			foreach (var s in ds)
			{
				if (result.Contains(s))
					result = result.Replace(s.ToString(), Replacements[s]);
			}

			var sb = new StringBuilder();
			bool active = false;

			for (var i = 0; i < result.Length; i++)
			{
				if (result[i] == '-')
				{
					if (active)
						continue;

					active = true;
					sb.Append(result[i]);
				}
				else
				{
					active = false;
					sb.Append(result[i]);
				}
			}

			return sb.ToString().Trim().Trim('-');
		}

		public static string GenerateUrl(string id, string text, List<IUrlRecord> existing)
		{
			return GenerateUrl(id, text, existing, true);
		}

		public static string GenerateUrl(string id, string text, List<IUrlRecord> existing, bool allowUnderscore)
		{
			if (string.IsNullOrEmpty(text))
				return null;

			var result = text.Trim();

			if (!allowUnderscore)
				result = result.Replace('_', '-');

			result = result.Trim().Trim('-').ToLower().Replace(".", "-").Replace(' ', '-').Replace("\t", "-").Replace("\r", "-").Replace("\n", "-");

			var ds = Replacements.Keys.ToList();

			foreach (var s in ds)
			{
				if (result.Contains(s))
					result = result.Replace(s.ToString(), Replacements[s]);
			}

			var sb = new StringBuilder();
			var active = false;

			for (var i = 0; i < result.Length; i++)
			{
				if (result[i] == '-')
				{
					if (active)
						continue;

					active = true;
					sb.Append(result[i]);
				}
				else
				{
					active = false;
					sb.Append(result[i]);
				}
			}

			result = sb.ToString().Trim().Trim('-');

			if (existing == null || existing.Count == 0)
				return result;
			else
			{
				if (!string.IsNullOrWhiteSpace(id))
				{
					var d = existing.FirstOrDefault(f => string.Compare(f.Id, id, true) == 0);

					if (d != null)
					{
						var idx = d.Url.LastIndexOf('-');

						if (idx > 0)
						{
							var origin = d.Url.Substring(0, idx);

							if (string.Compare(result, origin, true) == 0)
								return d.Url;
						}
					}
				}

				var exists = false;

				foreach (var d in existing)
				{
					if (string.Compare(d.Id, id, true) == 0)
						continue;

					if (string.Compare(d.Url, result, true) == 0)
					{
						exists = true;
						break;
					}
				}

				if (!exists)
					return result;
				else
					return Copy(result, 1, existing);
			}
		}

		private static string Copy(string text, int copyNumber, List<IUrlRecord> items)
		{
			foreach (var d in items)
			{
				if (string.Compare(string.Format("{0}-{1}", text, copyNumber), d.Url, true) == 0)
					return Copy(text, ++copyNumber, items);
			}

			return string.Format("{0}-{1}", text, copyNumber);
		}
	}
}