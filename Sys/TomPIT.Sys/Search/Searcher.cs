using System;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace TomPIT.Sys.Search
{
	internal class Searcher : IDisposable
	{
		private IndexSearcher _searcher = null;

		public Searcher(IndexWriter writer)
		{
			Reader = writer.GetReader();
		}

		public Searcher(Searcher searcher)
		{
			Reader = searcher.Reader.Reopen();
		}

		public void Dispose()
		{
			Kill();
		}

		private void Kill()
		{
			try
			{
				if (Reader != null)
					Reader.Dispose();

				if (_searcher != null)
					_searcher.Dispose();

				Reader = null;
				_searcher = null;
			}
			catch { }
		}

		public IndexReader Reader { get; private set; } = null;
		public IndexSearcher Index
		{
			get
			{
				if (_searcher == null && Reader != null)
					_searcher = new IndexSearcher(Reader);

				return _searcher;
			}
		}

		public void TryDispose()
		{
			if (Reader != null && Reader.RefCount < 2)
				Kill();
		}
	}
}