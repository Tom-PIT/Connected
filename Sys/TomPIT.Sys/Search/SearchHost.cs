using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using TomPIT.Diagnostics;
using TomPIT.Reflection;
using TomPIT.Search;
using TomPIT.Sys.Data;
using TomPIT.Sys.Diagnostics;

namespace TomPIT.Sys.Search
{
	internal abstract class SearchHost<T> : IDisposable
	{
		private readonly object sync = new object();
		private readonly object searcherLock = new object();

		private IndexWriter _writer = null;
		private FSDirectory _directory = null;
		//private bool _isValid = true;
		private int _activeOperations = 0;
		private Searcher _searcher = null;
		private ConcurrentBag<T> _messageBuffer = null;
		public abstract string FileName { get; }
		protected abstract string SearchDirectory { get; }

		private string Path { get { return System.IO.Path.Combine(SearchDirectory, FileName); } }

		private IndexWriter Writer
		{
			get
			{
				if (_writer == null)
				{
					lock (sync)
					{
						if (_writer == null)
							_writer = new IndexWriter(Directory, new WriteAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED);
					}
				}

				return _writer;
			}
		}

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

		public bool IsDisposing { get; private set; }

		public void Dispose()
		{
			IsDisposing = true;

			while (IsActive)
				Thread.Sleep(10);

			Flush();

			IsDisposed = true;

			KillAll();
		}

		public bool IsDisposed { get; private set; }

		public void Flush()
		{
			if (MessageBuffer.Count == 0)
				return;

			try
			{
				lock (MessageBuffer)
				{
					OperationStart();
					Writer.Commit();

					while (MessageBuffer.TryTake(out T m))
						OnComplete(m);
				}

				OperationEnd();
			}
			catch (Exception ex)
			{
				//_isValid = false;
				OperationEnd();
				KillAll();

				LogError(SysLogEvents.SearchFlush, ex.Message);
			}
		}

		protected virtual void OnComplete(T message)
		{

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
				Writer.AddDocument(doc);

				//_isValid = true;
				OperationEnd();
			}
			catch
			{
				OperationEnd();
				//_isValid = false;
				KillAll();

				throw;
			}
		}

		public void Update(Term term, Document doc)
		{
			try
			{
				OperationStart();
				Writer.UpdateDocument(term, doc);

				//_isValid = true;
				OperationEnd();
			}
			catch
			{
				OperationEnd();
				//_isValid = false;

				KillAll();

				throw;
			}
		}

		public void Drop()
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

		public void Reset()
		{
			lock (sync)
			{
				try
				{
					OperationStart();

					KillWriter();

					_writer = new IndexWriter(Directory, new WriteAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);
					OperationEnd();
				}
				catch
				{
					//_isValid = false;
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
				Writer.DeleteDocuments(term);

				//_isValid = true;
				OperationEnd();
			}
			catch
			{
				OperationEnd();
				//_isValid = false;

				KillAll();

				throw;
			}
		}

		private Searcher Searcher
		{
			get
			{
				if (_searcher == null)
				{
					lock (sync)
					{
						if (_searcher == null)
							_searcher = new Searcher(Writer);
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

						_searcher = new Searcher(Searcher);

						old.TryDispose();
					}
				}
			}
		}

		private void KillSearcher()
		{
			try
			{
				if (_searcher != null)
				{
					_searcher.Dispose();
					_searcher = null;
				}
			}
			catch { }
			finally
			{
				_searcher = null;
			}
		}

		private void KillWriter()
		{
			try
			{
				if (_writer != null)
				{
					_writer.Dispose();
					_writer = null;

					KillSearcher();
				}
			}
			catch { }
			finally
			{
				_writer = null;
			}
		}

		private void KillDirectory()
		{
			try
			{
				if (_directory != null)
					_directory.Dispose();
			}
			catch { }
			finally { _directory = null; }
		}

		public bool Exists { get { return Directory.Directory.Exists; } }

		public (SearchResultDocuments, int) Search(ISearchOptions options)
		{
			SearchJob job = null;

			switch (options.Kind)
			{
				case CommandKind.Term:
					job = new TermSearchJob(options);

					break;
				case CommandKind.Query:
					job = new QuerySearchJob(options);

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
			catch (Exception ex)
			{
				LogError(SysLogEvents.Search, ex.Message);

				//_isValid = false;

				KillAll();
			}
			finally
			{
				try
				{
					if (Searcher != job.Searcher)
						Searcher.TryDispose();
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

		private ConcurrentBag<T> MessageBuffer
		{
			get
			{
				if (_messageBuffer == null)
					_messageBuffer = new ConcurrentBag<T>();

				return _messageBuffer;
			}
		}

		public void SaveMessage(T message)
		{
			if (MessageBuffer.Contains(message))
				return;

			MessageBuffer.Add(message);
		}

		protected void LogError(int eventId, string message)
		{
			DataModel.Logging.Insert(new List<ILogEntry>
			{
				new LogEntry
				{
					Category = SysLogCategories.Search,
					EventId = eventId,
					Level = System.Diagnostics.TraceLevel.Error,
					Message = message,
					Source = GetType().ShortName()
				}
			});
		}
	}
}