﻿using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Lucene.Net.QueryParsers;
using TomPIT.Annotations.Search;
using TomPIT.ComponentModel.Search;
using TomPIT.Reflection;
using TomPIT.Search.Analyzers;

namespace TomPIT.Search.Catalogs
{
	internal class TermSearchJob : SearchJob
	{
		private static readonly List<string> SpecialCharacters = new List<string> { @"\", "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", "\"", "~", "?", ":" };
		private static Regex _rx = null;

		public TermSearchJob(ISearchCatalogConfiguration catalog, ISearchOptions options) : base(catalog, options)
		{
		}

		protected override string ParseCommandText()
		{
			var commandText = PrepareCommandText(EscapeCommandText(HttpUtility.HtmlDecode(Options.CommandText)));

			var sb = new StringBuilder();

			sb.AppendFormat("(");

			var properties = Catalog.CatalogProperties();
			var first = true;

			if (properties != null)
			{
				foreach (var property in properties)
				{
					if (!property.CanRead || !property.GetMethod.IsPublic)
						continue;

					var att = property.FindAttribute<SearchStoreAttribute>();

					if (att == null || !att.Enabled)
						continue;

					if (string.Compare(property.Name, SearchUtils.FieldLcid, true) == 0)
						continue;

					if (!first)
						sb.Append(" OR ");

					sb.Append($" {property.Name.ToLowerInvariant()}:{CommandText}");

					first = false;
				}
			}

			var customProperties = Catalog.CatalogCustomProperties();

			if (customProperties != null)
			{
				foreach (var property in customProperties)
				{
					if (!first)
						sb.Append(" OR ");

					sb.Append($" {property.ToLowerInvariant()}:{commandText}");

					first = false;
				}
			}

			sb.Append(")");

			sb.Append($" AND {SearchUtils.FieldLcid}:{Options.Globalization.Lcid.ToString()}");

			return sb.ToString();
		}

		protected override QueryParser CreateParser()
		{
			var fields = new List<string>
			{
				SearchUtils.FieldLcid,
				SearchUtils.FieldTitle,
				SearchUtils.FieldTags
			};

			var properties = Catalog.CatalogProperties();

			if (properties != null)
			{
				foreach (var property in properties)
				{
					if (property.CanRead && property.GetMethod.IsPublic)
						fields.Add(property.Name.ToLowerInvariant());
				}
			}

			var customProperties = Catalog.CatalogCustomProperties();

			if (customProperties != null)
			{
				foreach (var property in customProperties)
					fields.Add(property.ToLowerInvariant());
			}

			return new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields.ToArray(), new ReadAnalyzer(AnalyzerContext.Term));
		}

		protected override string PrepareCommandText(string commandText)
		{
			return commandText;
		}

		private static Regex regex
		{
			get
			{
				if (_rx == null)
					_rx = new Regex(@"\w+", RegexOptions.IgnoreCase);

				return _rx;
			}
		}

		private string EscapeCommandText(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			foreach (var item in SpecialCharacters)
				value = value.Replace(item, $@"\{item}");

			return value;
		}

	}
}