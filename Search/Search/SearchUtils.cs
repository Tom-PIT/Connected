using System;
using System.Collections.Generic;
using System.Reflection;
using Lucene.Net.Documents;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Search
{
	internal static class SearchUtils
	{
		public const string FieldKey = "key";
		public const string FieldLcid = "lcid";
		public const string FieldTitle = "title";
		public const string FieldText = "text";
		public const string FieldTags = "tags";
		public const string FieldType = "type";
		public const string FieldAuthor = "author";
		public const string FieldDate = "date";

		public static bool IsSystemField(string fieldName)
		{
			return string.Compare(fieldName, FieldKey, true) == 0
				 || string.Compare(fieldName, FieldLcid, true) == 0
				 || string.Compare(fieldName, FieldTitle, true) == 0
				 || string.Compare(fieldName, FieldAuthor, true) == 0
				 || string.Compare(fieldName, FieldDate, true) == 0
				 || string.Compare(fieldName, FieldTags, true) == 0
				 || string.Compare(fieldName, FieldType, true) == 0;
		}
		public static bool IsStaticField(string fieldName)
		{
			return string.Compare(fieldName, FieldKey, true) == 0
				 || string.Compare(fieldName, FieldLcid, true) == 0
				 || string.Compare(fieldName, FieldAuthor, true) == 0
				 || string.Compare(fieldName, FieldDate, true) == 0;
		}

		public static ISearchMiddleware<T> CreateHandler<T>(this ISearchCatalogConfiguration catalog)
		{
			var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(((IConfiguration)catalog).MicroService(), catalog, catalog.ComponentName());

			if (type == null)
				return null;

			return type.CreateInstance<ISearchMiddleware<T>>();
		}

		public static List<PropertyInfo> CatalogProperties(this ISearchCatalogConfiguration catalog)
		{
			var type = catalog.CatalogType();

			if (type == null)
				return null;

			var properties = type.GetProperties();
			var result = new List<PropertyInfo>();

			foreach (var property in properties)
			{
				if (!property.CanRead)
					continue;

				if (!property.GetMethod.IsPublic)
					continue;

				if (!property.PropertyType.IsTypePrimitive())
					continue;

				result.Add(property);
			}

			return result;
		}

		public static List<string> CatalogCustomProperties(this ISearchCatalogConfiguration catalog)
		{
			var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(catalog.MicroService(), catalog, catalog.ComponentName(), false);

			if (type == null)
				return null;

			var instance = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<ISearchComponent>(new MicroServiceContext(catalog.MicroService()), type);

			if (instance != null)
				return instance.Properties;

			return null;
		}

		public static Field.Index ToFieldIndex(SearchMode mode)
		{
			return mode switch
			{
				SearchMode.Analyzed => Field.Index.ANALYZED,
				SearchMode.AnalyzedNoNorms => Field.Index.ANALYZED_NO_NORMS,
				SearchMode.No => Field.Index.NO,
				SearchMode.NotAnalyzed => Field.Index.NOT_ANALYZED,
				SearchMode.NotAnalyzedNoNorms => Field.Index.NOT_ANALYZED_NO_NORMS,
				_ => throw new NotSupportedException()
			};
		}

		public static Field.TermVector ToTermVector(SearchTermVector vector)
		{
			return vector switch
			{
				SearchTermVector.No => Field.TermVector.NO,
				SearchTermVector.WithOffsets => Field.TermVector.WITH_OFFSETS,
				SearchTermVector.WithPositions => Field.TermVector.WITH_POSITIONS,
				SearchTermVector.WithPositionsAndOffsets => Field.TermVector.WITH_POSITIONS_OFFSETS,
				SearchTermVector.Yes => Field.TermVector.YES,
				_ => throw new NotSupportedException()
			};
		}
	}
}
