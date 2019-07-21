using Lucene.Net.QueryParsers;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Search;
using TomPIT.Search.Analyzers;

namespace TomPIT.Search.Catalogs
{
    internal class QuerySearchJob : SearchJob
    {
        public QuerySearchJob(ISearchCatalog catalog, ISearchOptions options) : base(catalog, options)
        {
        }

        protected override string ParseCommandText()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("({0})", CommandText);
            sb.AppendFormat(" AND {0}:{1}", SearchUtils.FieldLcid, Options.Globalization.Lcid.AsString());

            return sb.ToString();
        }

		protected override MultiFieldQueryParser CreateParser()
		{
			var fields = new List<string>
			{
				SearchUtils.FieldLcid,
				SearchUtils.FieldTitle,
				SearchUtils.FieldTags,
				SearchUtils.FieldDate,
				SearchUtils.FieldKey,
				SearchUtils.FieldAuthor
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
            return commandText;
        }
    }
}
