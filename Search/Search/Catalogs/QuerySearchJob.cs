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

			sb.AppendFormat("{0}", CommandText);
			sb.AppendFormat(" AND {0}:{1}", SearchUtils.FieldLcid, Options.Globalization.Lcid.ToString());

			return sb.ToString();
		}

		protected override QueryParser CreateParser()
		{
			return new QueryParser(Lucene.Net.Util.Version.LUCENE_30, SearchUtils.FieldText, new ReadAnalyzer(AnalyzerContext.Query));
		}

		protected override string PrepareCommandText(string commandText)
		{
			return commandText;
		}
	}
}
