using System;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	internal class DomElementActivator
	{
		public DomElementActivator(IEnvironment environment, IDomElement parent, object instance, PropertyInfo pi, int index)
		{
			Environment = environment;
			Parent = parent;
			Instance = instance;
			Property = pi;
			Index = index;
		}

		private IEnvironment Environment { get; }
		private IDomElement Parent { get; }
		private object Instance { get; }
		private PropertyInfo Property { get; }
		private int Index { get; }

		public IDomElement CreateInstance()
		{
			var type = ResolveType();

			if (type == null)
				return null;

			var constructors = type.GetConstructors();

			foreach (var i in constructors)
			{
				var pars = i.GetParameters();

				if (pars.Length == 0)
					continue;

				if (pars.Length == 1 && pars[0].ParameterType == typeof(ReflectorCreateArgs))
					return CreateReflectorElement(type);
				else if (pars.Length == 3 && pars[2].ParameterType == typeof(IComponent))
					return CreateComponentElement(type);
				else if (pars.Length == 2)
					return CreateDefaultElement(type);
			}

			return null;
		}

		private IDomElement CreateDefaultElement(Type type)
		{
			return type.CreateInstance<DomElement>(new object[] { Parent.Environment, Parent });
		}

		private IDomElement CreateComponentElement(Type type)
		{
			return type.CreateInstance<DomElement>(new object[] { Parent.Environment, Parent, ResolveComponent() });
		}

		private IDomElement CreateReflectorElement(Type type)
		{
			var args = new ReflectorCreateArgs
			{
				Environment = Parent.Environment,
				Instance = Instance,
				Property = Property,
				Parent = Parent,
				Index = Index
			};

			return type.CreateInstance<DomElement>(new object[] { args });
		}

		private Type ResolveType()
		{
			if (Property != null)
			{
				var element = Property.AttributeLookup<DomElementAttribute>();

				if (element != null)
					return element.Type ?? Type.GetType(element.TypeName);

				return null;
			}

			if (Instance != null)
			{
				var element = Instance.GetType().FindAttribute<DomElementAttribute>();

				if (element != null)
					return element.Type ?? Type.GetType(element.TypeName);
			}

			return null;
		}

		private IComponent ResolveComponent()
		{
			if (Instance == null)
				return null;

			if (Instance is IComponent)
				return Instance as IComponent;

			if (Instance is IConfiguration cfg)
				return Environment.Context.Connection().GetService<IComponentService>().SelectComponent(cfg.Component);
			else if (Instance is IElement e)
				return Environment.Context.Connection().GetService<IComponentService>().SelectComponent(e.Configuration().Component);
			else
				return null;
		}
	}
}
