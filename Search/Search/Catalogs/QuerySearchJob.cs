using System.Collections.Generic;
using System.Text;
using Lucene.Net.QueryParsers;
using TomPIT.ComponentModel.Search;
using TomPIT.Search.Analyzers;

namespace TomPIT.Search.Catalogs
{
	internal class QuerySearchJob : SearchJob
	{
		public QuerySearchJob(ISearchCatalogConfiguration catalog, ISearchOptions options) : base(catalog, options)
		{
		}

		protected override string ParseCommandText()
		{
			var sb = new StringBuilder();

			sb.AppendFormat("({0})", CommandText);
			sb.AppendFormat(" AND {0}:{1}", SearchUtils.FieldLcid, Options.Globalization.Lcid.ToString());

			return sb.ToString();
		}

		protected override MultiFieldQueryParser CreateParser()
		{
			var fields = new List<string>();
			var properties = Catalog.CatalogProperties();

			if (properties != null)
			{
				foreach (var property in properties)
				{
					if (string.Compare(property.Name, SearchUtils.FieldLcid, true) == 0)
						continue;

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

			return new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields.ToArray(), new ReadAnalyzer());
		}

		protected override string PrepareCommandText(string commandText)
		{
			return commandText;
		}
	}
}
