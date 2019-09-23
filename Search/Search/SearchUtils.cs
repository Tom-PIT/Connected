using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
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
		public const string FieldAuthor = "author";
		public const string FieldDate = "date";

		public static bool IsSystemField(string fieldName)
		{
			return string.Compare(fieldName, FieldKey, true) == 0
				 || string.Compare(fieldName, FieldLcid, true) == 0
				 || string.Compare(fieldName, FieldTitle, true) == 0
				 || string.Compare(fieldName, FieldAuthor, true) == 0
				 || string.Compare(fieldName, FieldDate, true) == 0
				 || string.Compare(fieldName, FieldTags, true) == 0;
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
			var type = Instance.Tenant.GetService<ICompilerService>().ResolveType(((IConfiguration)catalog).MicroService(), catalog, catalog.ComponentName());

			if (type == null)
				return null;

			return type.CreateInstance<ISearchMiddleware<T>>();
		}

		public static Type CatalogType(this ISearchCatalogConfiguration catalog)
		{
			var type = Instance.Tenant.GetService<ICompilerService>().ResolveType(((IConfiguration)catalog).MicroService(), catalog, catalog.ComponentName());

			if (type == null)
				return null;

			var searchHandler = type.GetInterface(typeof(ISearchMiddleware<>).FullName);

			if (searchHandler == null)
				return null;

			return searchHandler.GetGenericArguments()[0];
		}

		public static List<PropertyInfo> CatalogProperties(this ISearchCatalogConfiguration catalog)
		{
			var type = CatalogType(catalog);

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

				result.Add(property);
			}

			return result;
		}
	}
}
