using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Search.Catalogs
{
	internal class SearchTextParser
	{
		private static readonly Regex Regex = null;
		private IMicroService _microService = null;
		private IComponent _catalog = null;
		private IMiddlewareContext _context = null;
		private CultureInfo Culture { get; set; }

		static SearchTextParser()
		{
			Regex = new Regex(@"\[(.*?)\]: \[(.*?)\]");
		}

		public SearchTextParser(SearchResultDocuments container, SearchResult descriptor, ISearchOptions options)
		{
			Container = container;
			Options = options;
			Descriptor = descriptor;

			SetCulture();
		}

		public void Parse()
		{
			if (Options.Highlight.Enabled)
				Descriptor.Text = HighlightedText(Container, Descriptor.Text);
			else
			{
				Descriptor.Text = Parse(Descriptor.Text);

				if (Options.Results.TextMaxLength > 0)
				{
					if (Descriptor.Text.Length > Options.Results.TextMaxLength)
						Descriptor.Text = Descriptor.Text.Substring(0, Descriptor.Text.Length);
				}
			}

			Descriptor.Title = Parse(Descriptor.Title);
		}

		private SearchResultDocuments Container { get; set; }
		private ISearchOptions Options { get; set; }
		private SearchResult Descriptor { get; }
		private string HighlightedText(SearchResultDocuments container, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return string.Empty;

			var c = Parse(value);

			var r = container.Highlight(c, 128);
			if (string.IsNullOrWhiteSpace(r))
			{
				if (string.IsNullOrWhiteSpace(c) || c.Length < 255)
					return c;
				else
					return c.Substring(0, 255);
			}

			return r;
		}

		private string Parse(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw))
				return raw;

			return Regex.Replace(raw, OnReplace);
		}

		private string OnReplace(Match match)
		{
			if (match == null || string.IsNullOrEmpty(match.Value))
				return null;

			var tokens = match.Value.Split(":".ToCharArray(), 2);

			if (tokens.Length != 2)
				return match.Value;

			var token = new SearchToken();

			token.Deserialize(tokens[0]);

			string title = string.Empty;

			if (!string.IsNullOrWhiteSpace(token.Key))
				title = Context.Services.Globalization.TryGetString(token.StringTable, token.Key, Culture.LCID);

			if (string.IsNullOrWhiteSpace(title))
				title = token.Key;

			string val = tokens[1].Trim();

			string formattedValue = FormatValue(val.Substring(1, val.Length - 2), token.Format);

			return string.Format("{0}: {1}", title, formattedValue);
		}

		private string FormatValue(string value, string format)
		{
			if (string.IsNullOrWhiteSpace(format))
				return value;

			if (string.Compare(format, "d", true) == 0 || string.Compare(format, "g", true) == 0 || string.Compare(format, "t", true) == 0)
			{
				if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
					return dt.ToString(format, Culture);
			}
			else if (format.StartsWith("n"))
			{
				if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double db))
					return db.ToString(format, Culture);
			}

			return value;
		}


		private IMiddlewareContext Context
		{
			get
			{
				if (_context == null)
					_context = MiddlewareDescriptor.Current.CreateContext(Catalog.MicroService);

				return _context;
			}
		}

		private IComponent Catalog
		{
			get
			{
				if (_catalog == null)
					_catalog = Instance.Tenant.GetService<IComponentService>().SelectComponent(Descriptor.Catalog);

				return _catalog;
			}
		}

		private IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = Instance.Tenant.GetService<IMicroServiceService>().Select(Catalog.MicroService);

				return _microService;
			}
		}

		private void SetCulture()
		{
			if (Options.Globalization.UICulture <= 0)
				Culture = CultureInfo.InvariantCulture;
			else
			{
				try
				{
					Culture = CultureInfo.GetCultureInfo(Options.Globalization.UICulture);
				}
				catch
				{
					Culture = CultureInfo.InvariantCulture;
				}
			}
		}
	}
}
