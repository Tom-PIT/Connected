﻿using System;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace TomPIT.Search.Catalogs
{
    internal class CatalogSearcher : IDisposable
    {
        private IndexReader _reader = null;
        private IndexSearcher _searcher = null;

        public CatalogSearcher(IndexWriter writer)
        {
            _reader = writer.GetReader();
        }

        public CatalogSearcher(CatalogSearcher searcher)
        {
            _reader = searcher.Reader.Reopen();
        }

        public void Dispose()
        {
            Kill();
        }

        private void Kill()
        {
            try
            {
                _reader?.Dispose();
                _searcher?.Dispose();
            }
            catch { }
            finally
            {
                _reader = null;
                _searcher = null;
            }
        }

        public IndexReader Reader { get { return _reader; } }
        public IndexSearcher Searcher
        {
            get
            {
                if (_searcher == null && _reader != null)
                    _searcher = new IndexSearcher(_reader);

                return _searcher;
            }
        }

        public bool TryDispose()
        {
            if (_reader is null || _reader?.RefCount < 2)
                return false;

            Kill();
            return true;
        }
    }
}