using System.Collections.Generic;
using System.Text;
using Lucene.Net.QueryParsers;
using TomPIT.Search;

namespace TomPIT.Sys.Search
{
	internal class QuerySearchJob : SearchJob
	{
		public QuerySearchJob(ISearchOptions options) : base(options)
		{
		}

		protected override string ParseCommandText()
		{
			var sb = new StringBuilder();

			sb.AppendFormat("({0})", CommandText);

			return sb.ToString();
		}

		protected override MultiFieldQueryParser CreateParser()
		{
			var fields = new List<string>
			{
				SearchFields.Title,
				SearchFields.Content,
				SearchFields.ComponentName,
				SearchFields.ElementName,
				SearchFields.Tags
			};

			return new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields.ToArray(), new ReadAnalyzer());
		}

		protected override string PrepareCommandText(string commandText)
		{
			return commandText;
		}
	}
}
