﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Configuration;
using TomPIT.Diagnostics;
using TomPIT.Middleware;
using TomPIT.Search.Analyzers;
using TomPIT.Storage;

namespace TomPIT.Search.Catalogs
{
    internal class CatalogHost : IDisposable
    {
        private readonly object sync = new();
        private readonly object searcherLock = new();

        private IndexWriter _writer = null;
        private FSDirectory _directory = null;
        private bool _isDisposed = false;
        private bool _isValid = true;
        private int _activeOperations = 0;
        private CatalogSearcher _searcher = null;
        private ConcurrentBag<IQueueMessage> _messageBuffer = null;
        private bool _disposing = false;
        private string _searchDirectory = null;

        public CatalogHost(ISearchCatalogConfiguration catalog, TimeSpan duration, bool slidingExpiration)
        {
            Catalog = catalog;
            Duration = duration;
            SlidingExpiration = slidingExpiration;

            if (duration > TimeSpan.Zero)
                ExpirationDate = DateTime.UtcNow.AddTicks(duration.Ticks);
        }

        private TimeSpan Duration { get; }
        private bool SlidingExpiration { get; }
        private DateTime ExpirationDate { get; set; } = DateTime.MinValue;
        public string FileName { get { return Catalog.Component.ToString(); } }
        public ISearchCatalogConfiguration Catalog { get; }

        private string SearchDirectory
        {
            get
            {
                if (_searchDirectory is null)
                {
                    _searchDirectory = MiddlewareDescriptor.Current.Tenant.GetService<ISettingService>().GetValue<string>("SearchPath", null, null, null);

                    if (string.IsNullOrWhiteSpace(_searchDirectory))
                    {
                        MiddlewareDescriptor.Current.Tenant.LogError(nameof(CatalogHost), "'SearchPath' setting not defined", "Search");
                        throw new NullReferenceException();
                    }
                }

                return _searchDirectory;
            }
        }

        private string Path { get { return System.IO.Path.Combine(SearchDirectory, FileName); } }

        public void Hit()
        {
            if (SlidingExpiration && Duration > TimeSpan.Zero)
                ExpirationDate = DateTime.UtcNow.AddTicks(Duration.Ticks);
        }

        public bool Expired
        {
            get
            {
                if (IsActive)
                    return false;

                if (ExpirationDate == DateTime.MinValue)
                    return false;

                return ExpirationDate < DateTime.UtcNow;
            }
        }

        private IndexWriter Writer
        {
            get
            {
                if (_writer is null)
                {
                    lock (sync)
                    {
                        if (_writer is null)
                        {
                            _writer = new IndexWriter(Directory, new WriteAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED);
                            _writer.FileNotFoundException += OnFileNotFoundException;
                        }
                    }
                }

                return _writer;
            }
        }

        private void OnFileNotFoundException(object sender, EventArgs e)
        {
            MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Rebuild(Catalog.Component);

            _isValid = false;

            MiddlewareDescriptor.Current.Tenant.LogError(nameof(FileNotFoundException), $"Index Corrupted ({Catalog.Component})", "Search");
        }

        public bool IsValid => _isValid && MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().SelectState(Catalog.Component) is null;

        private FSDirectory Directory
        {
            get
            {
                if (_directory == null)
                {
                    lock (sync)
                    {
                        if (_directory == null)
                            _directory = new SimpleFSDirectory(new DirectoryInfo(Path));
                    }
                }

                return _directory;
            }
        }

        public bool IsDisposing => _disposing;

        public void Dispose()
        {
            _disposing = true;

            while (IsActive)
                Thread.Sleep(10);

            Flush();

            _isDisposed = true;

            KillAll();
        }

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public void Flush()
        {
            if (MessageBuffer.Count == 0)
                return;

            try
            {
                lock (MessageBuffer)
                {
                    OperationStart();
                    lock (sync)
                        Writer.Commit();

                    while (MessageBuffer.TryTake(out IQueueMessage m))
                        MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Complete(m.PopReceipt);
                }

                OperationEnd();
            }
            catch (FileNotFoundException ex)
            {
                _isValid = false;
                OperationEnd();

                MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Rebuild(Catalog.Component);

                KillAll();

                MiddlewareDescriptor.Current.Tenant.LogError(nameof(Flush), ex.Message, "Search");
            }
            catch (Exception ex)
            {
                _isValid = false;
                OperationEnd();
                KillAll();

                MiddlewareDescriptor.Current.Tenant.LogError(nameof(Flush), ex.Message, "Search");
            }
        }

        private void KillAll()
        {
            KillWriter();
            KillSearcher();
            KillDirectory();
        }

        public void Add(Document doc)
        {
            try
            {
                OperationStart();
                lock (sync)
                    Writer.AddDocument(doc);

                _isValid = true;
                OperationEnd();
            }
            catch
            {
                OperationEnd();
                _isValid = false;
                KillAll();

                throw;
            }
        }

        public void Update(Term term, Document doc)
        {
            try
            {
                OperationStart();
                lock (sync)
                    Writer.UpdateDocument(term, doc);

                _isValid = true;
                OperationEnd();
            }
            catch
            {
                OperationEnd();
                _isValid = false;

                KillAll();

                throw;
            }
        }

        public void Drop(CancellationToken cancel)
        {
            try
            {
                OperationStart();

                lock (sync)
                {
                    KillWriter();
                    KillSearcher();

                    Directory.Directory.Delete(true);

                    KillDirectory();
                }
            }
            finally
            {
                OperationEnd();
            }
        }

        public void Reset(CancellationToken cancel)
        {
            lock (sync)
            {
                try
                {
                    OperationStart();

                    KillWriter();

                    _writer = new IndexWriter(Directory, new WriteAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);
                    _writer.FileNotFoundException += OnFileNotFoundException;
                    OperationEnd();
                }
                catch
                {
                    _isValid = false;
                    OperationEnd();
                    KillAll();

                    throw;
                }
            }
        }

        public void Delete(Term term)
        {
            try
            {
                OperationStart();
                lock (sync)
                    Writer.DeleteDocuments(term);

                _isValid = true;
                OperationEnd();
            }
            catch
            {
                OperationEnd();
                _isValid = false;

                KillAll();

                throw;
            }
        }

        private CatalogSearcher Searcher
        {
            get
            {
                if (_searcher is null)
                {
                    lock (sync)
                    {
                        _searcher ??= new CatalogSearcher(Writer);
                    }
                }

                return _searcher;
            }
        }

        private void EnsureCurrent()
        {
            if (!Searcher.Reader.IsCurrent())
            {
                lock (searcherLock)
                {
                    if (!Searcher.Reader.IsCurrent())
                    {
                        var old = Searcher;

                        _searcher = new CatalogSearcher(Searcher);

                        old.TryDispose(); //Why is this?
                    }
                }
            }
        }

        private void KillSearcher()
        {
            lock (sync)
            {
                if (_searcher is null)
                    return;

                try
                {
                    _searcher.Dispose();
                }
                catch { }
                finally
                {
                    _searcher = null;
                }
            }
        }

        private void KillWriter()
        {
            lock (sync)
            {
                if (_writer is null)
                    return;

                try
                {
                    _writer.FileNotFoundException -= OnFileNotFoundException;

                    _writer.Dispose();

                    KillSearcher();
                }
                catch { }
                finally
                {
                    _writer = null;
                }
            }
        }

        private void KillDirectory()
        {
            lock (sync)
            {
                if (_directory is null)
                    return;

                try
                {
                    _directory.Dispose();
                }
                catch { }
                finally { _directory = null; }
            }
        }

        public bool Exists { get { return Directory.Directory.Exists; } }

        public (SearchResultDocuments, int) Search(ISearchOptions options)
        {
            SearchJob job = null;

            switch (options.Kind)
            {
                case CommandKind.Term:
                    job = new TermSearchJob(Catalog, options);

                    break;
                case CommandKind.Query:
                    job = new QuerySearchJob(Catalog, options);

                    break;
                default:
                    throw new NotImplementedException();
            }

            return Search(job, options);
        }

        private (SearchResultDocuments, int) Search(SearchJob job, ISearchOptions options)
        {
            var total = 0;

            if (!IndexReader.IndexExists(Directory))
                return (new SearchResultDocuments(options), 0);

            EnsureCurrent();

            try
            {
                job.Search(Searcher);

                total = job.Total;
            }
            catch (FileNotFoundException ex)
            {
                _isValid = false;

                MiddlewareDescriptor.Current.Tenant.LogError(nameof(Search), ex.Message, "Search");
                MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Rebuild(Catalog.Component);

                KillAll();

                throw new CorruptedIndexException(Catalog.Component);
            }
            catch (Exception ex)
            {
                MiddlewareDescriptor.Current.Tenant.LogError(nameof(Search), $"{Catalog.ComponentName()} ({job.CommandText}, {ex.Message})", "Search");

                _isValid = false;

                KillAll();
            }
            finally
            {
                try
                {
                    if (Searcher != job.Searcher)
                    {
                        if (Searcher.TryDispose())
                            KillSearcher();
                    }
                }
                catch { }
            }

            return (job.Results, total);
        }

        public bool IsActive
        {
            get { return _activeOperations > 0; }
        }

        private void OperationStart()
        {
            Interlocked.Increment(ref _activeOperations);
        }

        private void OperationEnd()
        {
            Interlocked.Decrement(ref _activeOperations);
        }

        private ConcurrentBag<IQueueMessage> MessageBuffer
        {
            get
            {
                if (_messageBuffer == null)
                    _messageBuffer = new ConcurrentBag<IQueueMessage>();

                return _messageBuffer;
            }
        }

        public void SaveMessage(IQueueMessage message)
        {
            if (MessageBuffer.Contains(message))
                return;

            MessageBuffer.Add(message);
        }
    }
}