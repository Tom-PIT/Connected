using System;
using System.Collections.Generic;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.IoC;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Search
{
	public abstract class SearchMiddleware<T> : MiddlewareOperation, ISearchMiddleware<T>
	{
		private List<ISearchDependencyInjectionMiddleware> _dependencies = null;
		private List<string> _customProperties = null;

		[SkipValidation]
		protected internal List<ISearchDependencyInjectionMiddleware> DependencyInjections
		{
			get
			{
				if (_dependencies == null)
				{
					var component = Context.Tenant.GetService<ICompilerService>().ResolveComponent(this);

					if (component != null)
					{
						var ms = Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

						_dependencies = Context.Tenant.GetService<IDependencyInjectionService>().QuerySearchDependencies($"{ms.Name}/{component.Name}", this);
					}

					if (_dependencies == null)
						_dependencies = new List<ISearchDependencyInjectionMiddleware>();
				}

				return _dependencies;
			}
		}

		public SearchVerb Verb { get; set; } = SearchVerb.Update;
		public virtual SearchValidationBehavior ValidationFailed => SearchValidationBehavior.Complete;

		[SkipValidation]
		public List<string> Properties
		{
			get
			{
				if (_customProperties == null)
				{
					_customProperties = new List<string>();

					foreach (var dependency in DependencyInjections)
					{
						if (dependency.Properties != null && dependency.Properties.Count > 0)
							_customProperties.AddRange(dependency.Properties);
					}
				}

				return _customProperties;
			}
		}

		public T Search(string searchResult)
		{
			try
			{
				Context.Grant();
				var result = OnSearch(searchResult);

				Invoked();

				return result;
			}
			catch (Exception ex)
			{
				Rollback();
				throw new ScriptException(this, ex);
			}
		}

		protected virtual T OnSearch(string searchResult)
		{
			var instance = Serializer.Deserialize<T>(searchResult);

			OnSearch(instance);

			if (!typeof(T).ImplementsInterface<ISearchEntity>())
				return instance;

			var result = instance as ISearchEntity;

			foreach (var dependency in DependencyInjections)
				result = dependency.Search(result, searchResult);

			return result == null ? default : (T)result;
		}

		protected virtual void OnSearch(T instance)
		{

		}

		public List<T> Index()
		{
			try
			{
				Context.Grant();
				var result = PerformIndex();

				Invoked();

				return result;
			}
			catch (Exception ex)
			{
				Rollback();
				throw new ScriptException(this, ex);
			}
		}

		private List<T> PerformIndex()
		{
			if (Verb != SearchVerb.Rebuild)
				Validate();

			var result = OnIndex();

			if (result == null)
				result = new List<T>();

			if (!(typeof(T).ImplementsInterface(typeof(ISearchEntity))))
				return result;

			var items = new List<ISearchEntity>();

			foreach (var item in result)
				items.Add(item as ISearchEntity);

			foreach (var dependency in DependencyInjections)
				items = dependency.Index(items == null ? new List<ISearchEntity>() : items);

			if (items == null)
				return null;

			result = new List<T>();

			foreach (var item in items)
				result.Add((T)item);

			return result;
		}

		protected virtual List<T> OnIndex()
		{
			return new List<T>();
		}
	}
}
