using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Lucene.Net.QueryParsers;
using TomPIT.Annotations.Search;
using TomPIT.ComponentModel.Search;
using TomPIT.Reflection;
using TomPIT.Search.Analyzers;

namespace TomPIT.Search.Catalogs
{
	internal class TermSearchJob : SearchJob
	{
		private static Regex _rx = null;

		public TermSearchJob(ISearchCatalogConfiguration catalog, ISearchOptions options) : base(catalog, options)
		{
		}

		protected override string ParseCommandText()
		{
			var sb = new StringBuilder();

			//sb.AppendFormat("({0}:{1}", SearchUtils.FieldTitle, CommandText);
			//sb.AppendFormat(" OR {0}:{1}", SearchUtils.FieldTags, CommandText);
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

			sb.Append(")");

			sb.Append($" AND {SearchUtils.FieldLcid}:{Options.Globalization.Lcid.ToString()}");

			return sb.ToString();
		}

		protected override MultiFieldQueryParser CreateParser()
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

			return new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields.ToArray(), new ReadAnalyzer());
		}

		protected override string PrepareCommandText(string commandText)
		{
			//var col = regex.Matches(commandText);
			//var r = string.Empty;

			//if (col != null && col.Count > 0)
			//{
			//	foreach (Match item in col)
			//		r = string.Concat(commandText, " ", item.Value);

			//	return r;
			//}
			//else
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

	}
}