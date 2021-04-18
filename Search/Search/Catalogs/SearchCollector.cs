using System;
using System.Reflection;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Runtime;
using TomPIT.Serialization;

namespace TomPIT.Search.Catalogs
{
	internal class SearchCollector : TopDocsCollector<ScoreDoc>
	{
		public SearchCollector(int count, IndexSearcher searcher, ISearchCatalogConfiguration configuration) : base(new HitQueue(count, true))
		{
			Top = pq.Top();
			Searcher = searcher;
			Configuration = configuration;
		}
		public override bool AcceptsDocsOutOfOrder => false;

		private ISearchCatalogConfiguration Configuration { get; }
		private ISearchComponent Middleware { get; set; }
		private ScoreDoc Top { get; set; }
		private IndexSearcher Searcher { get; }
		private bool MiddlewareInitialized { get; set; }
		private Type EntityType { get; set; }
		private MethodInfo AuthorizeMethod { get; set; }
		private int BaseId { get; set; }
		private Scorer Scorer { get; set; }
		public override void Collect(int doc)
		{
			if (!Authorize(doc + BaseId))
				return;

			var score = Scorer.Score();

			internalTotalHits++;

			if (score <= Top.Score)
				return;

			Top.Doc = doc + BaseId;
			Top.Score = score;
			Top = pq.UpdateTop();

		}

		public override TopDocs NewTopDocs(ScoreDoc[] results, int start)
		{
			if (results == null)
				return EMPTY_TOPDOCS;

			var maxScore = float.NaN;

			if (start == 0)
				maxScore = results[0].Score;
			else
			{
				for (var i = pq.Size(); i > 1; i--)
					pq.Pop();

				maxScore = pq.Pop().Score;
			}

			return new TopDocs(internalTotalHits, results, maxScore);
		}

		public override void SetNextReader(IndexReader reader, int docBase)
		{
			BaseId = docBase;
		}

		public override void SetScorer(Scorer scorer)
		{
			Scorer = scorer;
		}

		private bool Authorize(int doc)
		{
			InitializeMiddleware();

			if (EntityType == null)
				return true;

			var document = Searcher.Doc(doc);

			if (document == null)
				return true;

			var content = new JObject();
			var fields = document.GetFields();

			foreach (var field in fields)
				content.Add(field.Name, field.StringValue);

			var instance = Serializer.Deserialize(Serializer.Serialize(content), EntityType);

			if (instance != null && instance is ISearchEntity entity)
				return Types.Convert<bool>(AuthorizeMethod.Invoke(Middleware, new object[] { entity }));

			return true;
		}



		private void InitializeMiddleware()
		{
			if (MiddlewareInitialized)
				return;

			MiddlewareInitialized = true;

			var ctx = Configuration.CreateContext();
			var type = Configuration.Middleware(ctx);

			if (type == null)
				return;

			Middleware = ctx.CreateMiddleware<ISearchComponent>(type);

			if (Middleware == null)
				return;

			var itf = Middleware.GetType().GetInterface(typeof(ISearchMiddleware<>).FullName);

			if (itf == null)
				return;

			EntityType = itf.GetGenericArguments()[0];

			foreach (var method in Middleware.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if (string.Compare(method.Name, nameof(ISearchMiddleware<object>.Authorize), false) == 0)
				{
					AuthorizeMethod = method;
					break;
				}
			}
		}
	}
}
