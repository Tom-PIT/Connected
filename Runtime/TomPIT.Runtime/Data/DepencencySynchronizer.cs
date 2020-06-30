using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Data
{
	internal class DepencencySynchronizer : TenantObject
	{
		private IMicroServiceContext _context = null;
		private Type _type = null;
		public DepencencySynchronizer(ITenant tenant, ModelSchema schema, IModelConfiguration configuration) : base(tenant)
		{
			Schema = schema;
			Configuration = configuration;
		}

		private ModelSchema Schema { get; }
		private IModelConfiguration Configuration { get; }
		private IMicroServiceContext Context
		{
			get
			{
				if (_context == null)
					_context = new MicroServiceContext(Configuration.MicroService());

				return _context;
			}
		}

		private Type EntityType
		{
			get
			{
				if (_type == null)
					_type = Configuration.ModelType(Context);

				return _type;
			}
		}
		public void Synchronize()
		{
			var references = new List<Type>();

			if (!string.IsNullOrWhiteSpace(Schema.Dependency))
			{
				var type = EntityType.FindAttribute<DependencyAttribute>();

				if (type != null)
				{
					references.Add(type.Model);
					Synchronize(type.Model);
				}
			}

			var properties = EntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

			foreach (var property in properties)
			{
				var att = property.FindAttribute<DependencyAttribute>();

				if (att == null)
					continue;

				if (!att.Model.ImplementsInterface<IModelComponent>())
					continue;

				if (references.Contains(att.Model))
					continue;

				references.Add(att.Model);
				Synchronize(att.Model);
			}
		}

		private void Synchronize(Type model)
		{
			var component = Tenant.GetService<ICompilerService>().ResolveComponent(model);

			if (component == null)
				return;

			if (!(Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IModelConfiguration config))
				return;

			Tenant.GetService<IModelService>().SynchronizeEntity(config);
		}
	}
}
