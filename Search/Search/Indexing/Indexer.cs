﻿using Lucene.Net.Documents;
using Lucene.Net.Index;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel.Search;
using TomPIT.Search.Catalogs;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Search.Indexing
{
	internal class Indexer
	{
		private List<PropertyInfo> _properties = null;
		private MethodInfo _queryMethod = null;
		private ISearchProcessHandler _handler = null;
		public Indexer(ISearchCatalog catalog, IQueueMessage message, IIndexRequest request, SearchVerb verb, string args)
		{
			Catalog = catalog;
			Request = request;
			Verb = verb;
			Arguments = args;
			Queue = message;
		}

		private IQueueMessage Queue { get; }
		private ISearchCatalog Catalog { get; }
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

		private ISearchProcessHandler Handler
		{
			get
			{
				if (_handler == null)
				{
					var handlerType = Instance.GetService<ICompilerService>().ResolveType(Request.MicroService, Catalog, Request.Catalog);

					_handler = handlerType.CreateInstance<ISearchProcessHandler>(new object[] { Catalog.CreateContext() });

					if (!string.IsNullOrWhiteSpace(Arguments))
						Types.Populate(Arguments, _handler);
				}

				return _handler;
			}
		}

		private MethodInfo QueryMethod
		{
			get
			{
				if (_queryMethod == null && Handler !=null)
				{
					var queryMethods = Handler.GetType().GetMethods();

					foreach (var method in queryMethods)
					{
						if (string.Compare(method.Name, "Query", false) != 0)
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

		public void Index()
		{
			if (Verb == SearchVerb.Rebuild && Instance.GetService<IIndexingService>().SelectState(Catalog.Component) == null)
				return;

			try
			{
				OnIndex();
			}
			catch (Exception ex)
			{
				Instance.Connection.LogError("Search", nameof(Indexer), ex.Message);

				Success = false;
			}
		}

		private void OnIndex()
		{
			try
			{
				switch (Verb)
				{
					case SearchVerb.Add:
						Success = Insert();
						break;
					case SearchVerb.Remove:
						Success = Delete();
						break;
					case SearchVerb.Rebuild:
						var ci = Instance.GetService<IIndexingService>().SelectState(Catalog.Component);

						if (ci == null || ci.Status == CatalogStateStatus.Rebuilding)
						{
							Success = true;
							return;
						}

						Instance.GetService<IIndexingService>().Ping(Queue.PopReceipt, 3600);

						if (Rebuild())
						{
							Instance.GetService<IIndexingService>().CompleteRebuilding(Catalog.Component);
							Success = true;
						}
						else
							Success = false;
						break;
					case SearchVerb.Update:
						Success = Update();
						break;
					default:
						Success = false;
						break;
				}
			}
			catch(ValidationException valEx)
			{
				if (Handler.ValidationFailed == SearchValidationBehavior.Complete)
				{
					Instance.Connection.LogWarning(null, "Search", valEx.Source, valEx.Message);
					Success = true;
				}
				else
				{
					if (Verb == SearchVerb.Rebuild)
						Instance.GetService<IIndexingService>().Ping(Queue.PopReceipt, 60);

					Instance.Connection.LogError("Search", valEx.Source, valEx.Message);
				}
			}
			catch (Exception ex)
			{
				if (Verb == SearchVerb.Rebuild)
					Instance.GetService<IIndexingService>().Ping(Queue.PopReceipt, 60);

				Instance.Connection.LogError("Search", ex.Source, ex.Message);
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
					return value.ToString();
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

		private void AddValue(Document doc, PropertyInfo property, object value)
		{
			if (value == null || value == DBNull.Value)
				return;

			var storeAtt = property.FindAttribute<SearchStoreAttribute>();
			var indexAtt = property.FindAttribute<SearchModeAttribute>();
			var vectorAtt = property.FindAttribute<SearchTermVectorAttribute>();

			var store = storeAtt != null && storeAtt.Enabled ? Field.Store.YES : Field.Store.NO;
			var index = Field.Index.NO;
			var vector = Field.TermVector.NO;

			if (indexAtt != null)
			{
				switch (indexAtt.Mode)
				{
					case SearchMode.Analyzed:
						index = Field.Index.ANALYZED;
						break;
					case SearchMode.NotAnalyzed:
						index = Field.Index.NOT_ANALYZED;
						break;
					case SearchMode.NotAnalyzedNoNorms:
						index = Field.Index.NOT_ANALYZED_NO_NORMS;
						break;
					case SearchMode.AnalyzedNoNorms:
						index = Field.Index.ANALYZED_NO_NORMS;
						break;
					default:
						throw new NotSupportedException();
				}
			}

			if(vectorAtt != null)
			{
				switch (vectorAtt.Vector)
				{
					case SearchTermVector.Yes:
						vector = Field.TermVector.YES;
						break;
					case SearchTermVector.WithPositions:
						vector = Field.TermVector.WITH_POSITIONS;
						break;
					case SearchTermVector.WithOffsets:
						vector = Field.TermVector.WITH_OFFSETS;
						break;
					case SearchTermVector.WithPositionsAndOffsets:
						vector = Field.TermVector.WITH_POSITIONS_OFFSETS;
						break;
					default:
						break;
				}
			}

			var field = new Field(property.Name.ToLowerInvariant(), ConvertValue(Types.ToDataType(property.PropertyType), value), store, index, vector);

			var boost = property.FindAttribute<SearchBoostAttribute>();

			if (boost != null)
				field.Boost = boost.Boost;

			doc.Add(field);
		}

		protected bool Rebuild()
		{
			Instance.GetService<IIndexingService>().MarkRebuilding(Catalog.Component);

			try
			{
				if (Reset())
				{
					//TODO: implement rebuild logic (catalog rebuild property etc.)
					return true;
				}
			}
			catch
			{
				Instance.GetService<IIndexingService>().ResetRebuilding(Catalog.Component);
			}

			return false;
		}

		private void ProcessField(Document doc, CatalogHost catalog, object instance, PropertyInfo property)
		{
			var value = property.GetValue(instance);

			if (string.IsNullOrWhiteSpace(Types.Convert<string>(value)))
				return;

			AddValue(doc, property, value);
		}

		protected bool Insert()
		{
			var items = Items;

			if (items == null || items.Count == 0)
				return true;

			try
			{
				var catalog = GetCatalogHost();

				foreach (var i in items)
				{
					var doc = new Document();

					foreach (var property in Properties)
					{
						if (property.CanRead && property.GetMethod.IsPublic)
							ProcessField(doc, catalog, i, property);
					}

					EnsureLocale(doc);

					catalog.Add(doc);
				}

				catalog.SaveMessage(Queue);

				return true;
			}
			catch (Exception ex)
			{
				Instance.Connection.LogError("Search", nameof(Insert), ex.Message);

				return false;
			}
		}

		private bool Update()
		{
			var items = Items;

			if (items == null || items.Count == 0)
				return true;

			try
			{
				var catalog = GetCatalogHost();

				foreach (var i in items)
				{
					var doc = new Document();

					foreach (var property in Properties)
					{
						if (property.CanRead && property.GetMethod.IsPublic)
							ProcessField(doc, catalog, i, property);
					}

					var locale = 0;
					var pk = string.Empty;
					var lcidProperty = Properties.FirstOrDefault(f => string.Compare(f.Name, SearchUtils.FieldLcid, false) == 0);
					var idProperty = Properties.FirstOrDefault(f => string.Compare(f.Name, SearchUtils.FieldKey, false) == 0);

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

					catalog.Update(new Term(SearchUtils.FieldKey, pk), doc);
				}

				catalog.SaveMessage(Queue);

				return true;
			}
			catch (Exception ex)
			{
				Instance.Connection.LogError("Search", nameof(Update), ex.Message);

				return false;
			}
		}

		private bool Drop()
		{
			try
			{
				var c = IndexCache.Ensure(Catalog.Component);

				if (c == null)
					return true;

				c.Drop();

				return true;
			}
			catch (Exception ex)
			{
				Instance.Connection.LogError("Search", nameof(Update), ex.Message);

				return false;
			}
		}

		private bool Reset()
		{
			try
			{
				var c = GetCatalogHost();

				c.Reset();

				return true;
			}
			catch (Exception ex)
			{
				Instance.Connection.LogError("Search", nameof(Update), ex.Message);

				return false;
			}
		}

		protected bool Delete()
		{
			var items = Items;

			if (items == null || items.Count == 0)
				return true;

			try
			{
				var catalog = GetCatalogHost();
				var idProperty = Properties.FirstOrDefault(f => string.Compare(f.Name, SearchUtils.FieldKey, false) == 0);

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
				Instance.Connection.LogError("Search", nameof(Delete), ex.Message);

				return false;
			}
		}

		private void EnsureLocale(Document doc)
		{
			var field = doc.GetField(SearchUtils.FieldLcid);

			if (field == null)
				doc.Add(new Field(SearchUtils.FieldLcid, 0.AsString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO));
			else if (string.IsNullOrWhiteSpace(field.StringValue))
				field.SetValue(0.AsString());
		}
	}
}