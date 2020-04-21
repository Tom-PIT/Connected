using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using HtmlAgilityPack;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using TomPIT.Annotations.Search;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Search.Catalogs;
using TomPIT.Storage;

namespace TomPIT.Search.Indexing
{
	internal class Indexer
	{
		private List<PropertyInfo> _properties = null;
		private MethodInfo _queryMethod = null;
		private ISearchComponent _handler = null;
		public Indexer(ISearchCatalogConfiguration catalog, IQueueMessage message, IIndexRequest request, SearchVerb verb, string args)
		{
			Catalog = catalog;
			Request = request;
			Verb = verb;
			Arguments = args;
			Queue = message;
		}

		private IQueueMessage Queue { get; }
		private ISearchCatalogConfiguration Catalog { get; }
		private IIndexRequest Request { get; }
		private SearchVerb Verb { get; }
		private string Arguments { get; }
		public bool Success { get; private set; } = true;
		private List<PropertyInfo> Properties
		{
			get
			{
				if (_properties == null)
				{
					_properties = Catalog.CatalogProperties();

					if (_properties == null)
						_properties = new List<PropertyInfo>();
				}

				return _properties;
			}
		}

		private ISearchComponent Handler
		{
			get
			{
				if (_handler == null)
				{
					var handlerType = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(Request.MicroService, Catalog, Request.Catalog);

					_handler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<ISearchComponent>(new MicroServiceContext(Catalog.MicroService()), handlerType, Arguments);
				}

				return _handler;
			}
		}

		private MethodInfo QueryMethod
		{
			get
			{
				if (_queryMethod == null && Handler != null)
				{
					var queryMethods = Handler.GetType().GetMethods();

					foreach (var method in queryMethods)
					{
						if (string.Compare(method.Name, "Index", false) != 0)
							continue;

						if (!method.IsPublic || method.ContainsGenericParameters)
							continue;

						var parameters = method.GetParameters();

						if (parameters != null && parameters.Length > 0)
							continue;

						_queryMethod = method;

						break;
					}
				}

				return _queryMethod;
			}
		}

		private IList Items
		{
			get
			{
				if (QueryMethod == null)
					return new ArrayList();

				try
				{
					return (IList)QueryMethod.Invoke(Handler, null);
				}
				catch (Exception ex)
				{
					throw TomPITException.Unwrap(this, ex);
				}
			}
		}

		public void Index(CancellationToken cancel)
		{
			if (Verb == SearchVerb.Rebuild && MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().SelectState(Catalog.Component) == null)
				return;

			try
			{
				OnIndex(cancel);
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError("Search", nameof(Indexer), ex.Message);

				Success = false;
			}
		}

		private void OnIndex(CancellationToken cancel)
		{
			try
			{
				switch (Verb)
				{
					case SearchVerb.Add:
						Success = Insert(cancel);
						break;
					case SearchVerb.Remove:
						Success = Delete(cancel);
						break;
					case SearchVerb.Rebuild:
						var ci = MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().SelectState(Catalog.Component);

						if (ci == null || ci.Status == CatalogStateStatus.Rebuilding)
						{
							Success = true;
							return;
						}

						MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Ping(Queue.PopReceipt, 3600);

						if (Rebuild(cancel))
						{
							MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().CompleteRebuilding(Catalog.Component);
							Success = true;
						}
						else
							Success = false;
						break;
					case SearchVerb.Update:
						Success = Update(cancel);
						break;
					default:
						Success = false;
						break;
				}
			}
			catch (System.ComponentModel.DataAnnotations.ValidationException valEx)
			{
				if (Handler.ValidationFailed == SearchValidationBehavior.Complete)
				{
					MiddlewareDescriptor.Current.Tenant.LogWarning(valEx.Source, valEx.Message, LogCategories.Search);
					Success = true;
				}
				else
				{
					if (Verb == SearchVerb.Rebuild)
						MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Ping(Queue.PopReceipt, 60);

					MiddlewareDescriptor.Current.Tenant.LogError("Search", valEx.Source, valEx.Message);
				}
			}
			catch (Exception ex)
			{
				if (Verb == SearchVerb.Rebuild)
					MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().Ping(Queue.PopReceipt, 60);

				MiddlewareDescriptor.Current.Tenant.LogError("Search", ex.Source, ex.Message);
			}
		}

		protected CatalogHost GetCatalogHost()
		{
			var c = IndexCache.Ensure(Catalog.Component);

			if (c == null)
				throw new Exception($"Cannot retrieve catalog ({Request.Catalog}).");

			if (c.IsDisposed)
			{
				IndexCache.Remove(Catalog.Component);

				throw new Exception($"Catalog disposed ({c.FileName}).");
			}

			return c;
		}

		private string ConvertValue(DataType dataType, object value)
		{
			switch (dataType)
			{
				case DataType.String:
					return StripHtml(value.ToString());
				case DataType.Integer:
				case DataType.Long:
				case DataType.Float:
					return Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture);
				case DataType.Bool:
					return Convert.ToBoolean(value).ToString(CultureInfo.InvariantCulture);
				case DataType.Date:
					return Convert.ToDateTime(value).ToString("s");
				default:
					throw new NotSupportedException();
			}
		}

		private string StripHtml(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return value;

			if (!value.Contains("<"))
				return value;

			var doc = new HtmlDocument();

			doc.LoadHtml(value);

			if (doc == null)
				return value;

			return doc.DocumentNode.InnerText;

		}

		private void AddValue(Document doc, PropertyInfo property, object value)
		{
			if (value == null || value == DBNull.Value)
				return;

			var storeAtt = property.FindAttribute<SearchStoreAttribute>();
			var indexAtt = property.FindAttribute<SearchModeAttribute>();
			var vectorAtt = property.FindAttribute<SearchTermVectorAttribute>();

			var store = storeAtt == null ? Field.Store.YES : storeAtt.Enabled ? Field.Store.YES : Field.Store.NO;
			var index = indexAtt == null ? Field.Index.ANALYZED : SearchUtils.ToFieldIndex(indexAtt.Mode);
			var vector = vectorAtt == null ? Field.TermVector.NO : SearchUtils.ToTermVector(vectorAtt.Vector);
			var boost = property.FindAttribute<SearchBoostAttribute>();

			AddValue(doc, property.Name, ConvertValue(Types.ToDataType(property.PropertyType), value), store, index, vector, boost == null ? 0f : boost.Boost);
		}

		private void AddValue(Document doc, string fieldName, object value, Field.Store store, Field.Index index, Field.TermVector vector, float boost)
		{
			var field = new Field(fieldName.ToLowerInvariant(), ConvertValue(Types.ToDataType(value.GetType()), value), store, index, vector);

			if (boost != 0f)
				field.Boost = boost;

			doc.Add(field);
		}

		protected bool Rebuild(CancellationToken cancel)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().MarkRebuilding(Catalog.Component);

			try
			{
				if (Reset(cancel))
				{
					//TODO: implement rebuild logic (catalog rebuild property etc.)
					return true;
				}
			}
			catch
			{
				MiddlewareDescriptor.Current.Tenant.GetService<IIndexingService>().ResetRebuilding(Catalog.Component);
			}

			return false;
		}

		private void ProcessField(Document doc, CatalogHost catalog, ISearchField field)
		{
			if (string.IsNullOrWhiteSpace(field.Value))
				return;

			if (doc.GetField(field.Name) != null)
				return;

			AddValue(doc, field.Name, field.Value, Field.Store.YES, SearchUtils.ToFieldIndex(field.Mode), SearchUtils.ToTermVector(field.TermVector), field.Boost);
		}
		private void ProcessField(Document doc, CatalogHost catalog, object instance, PropertyInfo property)
		{
			if (!property.PropertyType.IsTypePrimitive())
				return;

			var value = property.GetValue(instance);

			if (string.IsNullOrWhiteSpace(Types.Convert<string>(value)))
				return;

			AddValue(doc, property, value);
		}

		protected bool Insert(CancellationToken cancel)
		{
			try
			{
				var catalog = GetCatalogHost();

				var items = Items;

				if (cancel.IsCancellationRequested)
					return false;

				if (items == null || items.Count == 0)
				{
					catalog.SaveMessage(Queue);
					return true;
				}

				var documents = new List<Document>();

				foreach (var i in items)
				{
					var doc = new Document();

					foreach (var property in Properties)
					{
						if (property.CanRead && property.GetMethod.IsPublic)
							ProcessField(doc, catalog, i, property);
					}

					if (i is ISearchEntity e)
					{
						foreach (var field in e.Properties)
							ProcessField(doc, catalog, field);
					}

					EnsureLocale(doc);

					documents.Add(doc);
				}

				if (cancel.IsCancellationRequested)
					return false;

				foreach (var document in documents)
					catalog.Add(document);

				catalog.SaveMessage(Queue);

				return true;
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError("Search", nameof(Insert), ex.Message);

				return false;
			}
		}

		private bool Update(CancellationToken cancel)
		{
			try
			{
				var catalog = GetCatalogHost();

				var items = Items;

				if (cancel.IsCancellationRequested)
					return false;

				if (items == null || items.Count == 0)
				{
					catalog.SaveMessage(Queue);
					return true;
				}

				var documents = new Dictionary<Term, Document>();

				foreach (var i in items)
				{
					var doc = new Document();

					foreach (var property in Properties)
					{
						if (property.CanRead && property.GetMethod.IsPublic)
							ProcessField(doc, catalog, i, property);
					}

					if (i is ISearchEntity e)
					{
						foreach (var field in e.Properties)
							ProcessField(doc, catalog, field);
					}

					var locale = 0;
					var pk = string.Empty;
					var lcidProperty = Properties.FirstOrDefault(f => string.Compare(f.Name, SearchUtils.FieldLcid, true) == 0);
					var idProperty = Properties.FirstOrDefault(f => string.Compare(f.Name, SearchUtils.FieldKey, true) == 0);

					if (lcidProperty != null)
					{
						object lcid = lcidProperty.GetValue(i);

						if (!Types.TryConvertInvariant(lcid, out locale))
							throw new Exception(string.Format("Cannot parse lcid field value '{0}'", lcid));
					}

					if (idProperty != null)
					{
						object primaryKey = idProperty.GetValue(i);

						if (!Types.TryConvertInvariant(primaryKey, out pk) || string.IsNullOrEmpty(pk))
							throw new Exception(string.Format("Id field value is null '{0}'", catalog.FileName));
					}

					EnsureLocale(doc);
					documents.Add(new Term(SearchUtils.FieldKey, pk), doc);
				}

				if (cancel.IsCancellationRequested)
					return false;

				foreach (var entry in documents)
					catalog.Update(entry.Key, entry.Value);

				catalog.SaveMessage(Queue);

				return true;
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError("Search", nameof(Update), ex.Message);

				return false;
			}
		}

		private bool Drop(CancellationToken cancel)
		{
			try
			{
				var c = IndexCache.Ensure(Catalog.Component);

				if (cancel.IsCancellationRequested)
					return false;

				if (c == null)
					return true;

				c.Drop(cancel);

				return true;
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError("Search", nameof(Update), ex.Message);

				return false;
			}
		}

		private bool Reset(CancellationToken cancel)
		{
			try
			{
				var c = GetCatalogHost();

				c.Reset(cancel);

				return true;
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError("Search", nameof(Update), ex.Message);

				return false;
			}
		}

		protected bool Delete(CancellationToken cancel)
		{
			try
			{
				var catalog = GetCatalogHost();
				var items = Items;

				if (items == null || items.Count == 0)
				{
					catalog.SaveMessage(Queue);
					return true;
				}

				var idProperty = Properties.FirstOrDefault(f => string.Compare(f.Name, SearchUtils.FieldKey, true) == 0);

				foreach (var i in items)
				{
					var pk = string.Empty;

					if (idProperty != null)
					{
						var primaryKey = idProperty.GetValue(i);

						if (!Types.TryConvertInvariant(primaryKey, out pk) || string.IsNullOrEmpty(pk))
							throw new Exception(string.Format("Id field value is null '{0}'", catalog.FileName));

						pk = primaryKey.ToString();
					}

					if (!string.IsNullOrWhiteSpace(pk))
						catalog.Delete(new Term(SearchUtils.FieldKey, pk));
				}

				catalog.SaveMessage(Queue);

				return true;
			}
			catch (Exception ex)
			{
				MiddlewareDescriptor.Current.Tenant.LogError("Search", nameof(Delete), ex.Message);

				return false;
			}
		}

		private void EnsureLocale(Document doc)
		{
			var field = doc.GetField(SearchUtils.FieldLcid);

			if (field == null)
				doc.Add(new Field(SearchUtils.FieldLcid, 0.ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO));
			else if (string.IsNullOrWhiteSpace(field.StringValue))
				field.SetValue(0.ToString());
		}
	}
}