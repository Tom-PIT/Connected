using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Annotations.Models;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Connectivity;
using TomPIT.Data.DataProviders;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Data
{
	internal class ModelService : TenantObject, IModelService
	{
		public ModelService(ITenant tenant) : base(tenant)
		{
		}

		public void SynchronizeEntity(IModelConfiguration configuration)
		{
			SynchronizeSchema(configuration, CreateSchemas(configuration));
		}

		private void SynchronizeSchema(IModelConfiguration configuration, List<IModelSchema> schemas)
		{
			var connections = ResolveConnections(configuration);
			using var ctx = new MicroServiceContext(configuration.MicroService());

			foreach (var cs in connections)
			{
				if (cs.DataProvider == Guid.Empty)
					throw new RuntimeException(SR.ErrConnectionDataProviderNotSet);

				var provider = Tenant.GetService<IDataProviderService>().Select(cs.DataProvider);

				if (provider == null)
					throw new RuntimeException(SR.ErrConnectionDataProviderNotFound);

				var procedures = new List<IModelOperationSchema>();

				foreach (var operation in configuration.Operations)
				{
					var text = Tenant.GetService<IComponentService>().SelectText(ctx.MicroService.Token, operation);

					if (!string.IsNullOrWhiteSpace(text))
					{
						procedures.Add(new ModelOperationSchema
						{
							Text = text
						});
					}
				}

				var views = new List<IModelOperationSchema>();

				foreach (var operation in configuration.Views)
				{
					var text = Tenant.GetService<IComponentService>().SelectText(ctx.MicroService.Token, operation);

					if (!string.IsNullOrWhiteSpace(text))
					{
						views.Add(new ModelOperationSchema
						{
							Text = text
						});
					}
				}

				if (provider is IOrmProvider orm)
					orm.Synchronize(cs.Value, schemas, views, procedures);
			}
		}

		private List<IModelSchema> CreateSchemas(IModelConfiguration configuration)
		{
			using var ctx = new MicroServiceContext(configuration.MicroService());
			var type = configuration.Middleware(ctx);
			var result = new List<IModelSchema>
			{
				DiscoverSchema(configuration.ModelType(ctx))
			};

			var instance = ctx.CreateMiddleware<IModelComponent>(type);
			var entities = instance.QueryEntities();

			if (entities == null)
				return result;

			foreach (var entity in entities)
				result.Add(DiscoverSchema(entity));

			return result;
		}

		private ModelSchema DiscoverSchema(Type type)
		{
			var properties = ConfigurationExtensions.GetMiddlewareProperties(type, true);
			var result = new ModelSchema();

			var schema = type.FindAttribute<SchemaAttribute>();
			var ignore = type.FindAttribute<IgnoreAttribute>();

			schema.Ignore = ignore != null;

			result.Name = type.ShortName();
			result.Type = "Table";

			if (schema != null)
			{
				result.Schema = schema.Schema;

				if (!string.IsNullOrWhiteSpace(schema.Name))
					result.Name = schema.Name;

				if (!string.IsNullOrWhiteSpace(schema.Type))
					result.Type = schema.Type;

				if (string.Compare(result.Type, SchemaAttribute.SchemaTypeView, true) == 0)
					result.Ignore = true;
			}

			var dependency = type.FindAttribute<DependencyAttribute>();

			if (dependency != null)
				result.Dependency = dependency.Model.Name;

			var columns = new List<ModelSchemaColumn>();

			foreach (var property in properties)
			{
				if (!property.CanWrite)
					continue;

				if (property.FindAttribute<IgnoreAttribute>() != null)
					continue;

				var column = new ModelSchemaColumn(result)
				{
					Name = ResolveColumnName(property),
					DataType = Types.ToDbType(property)
				};

				var pk = property.FindAttribute<PrimaryKeyAttribute>();

				if (pk != null)
				{
					column.IsPrimaryKey = true;
					column.IsIdentity = pk.Identity;
					column.IsUnique = true;
					column.IsIndex = true;
				}

				var idx = property.FindAttribute<IndexAttribute>();

				if (idx != null)
				{
					column.IsIndex = true;
					column.IsUnique = idx.Unique;
					column.IndexGroup = idx.Group;
				}

				var ordinal = property.FindAttribute<OrdinalAttribute>();

				if (ordinal != null)
					column.Ordinal = ordinal.Ordinal;

				if (column.DataType == DbType.Decimal
					|| column.DataType == DbType.VarNumeric)
				{
					var numeric = property.FindAttribute<NumericAttribute>();

					if (numeric != null)
					{
						column.Precision = numeric.Percision;
						column.Scale = numeric.Scale;
					}
					else
					{
						column.Precision = 20;
						column.Scale = 5;
					}

				}
				else if (column.DataType == DbType.Date
					|| column.DataType == DbType.DateTime
					|| column.DataType == DbType.DateTime2
					|| column.DataType == DbType.DateTimeOffset
					|| column.DataType == DbType.Time)
				{
					var att = property.FindAttribute<DateAttribute>();

					if (att != null)
					{
						column.DateKind = att.Kind;
						column.DatePrecision = att.Precision;
					}
					else
					{
						column.DateKind = DateKind.DateTime2;
						column.DatePrecision = 7;
					}
				}
				else if (column.DataType == DbType.Binary)
				{
					var bin = property.FindAttribute<BinaryAttribute>();

					if (bin != null)
						column.BinaryKind = bin.Kind;
				}

				var def = property.FindAttribute<DefaultAttribute>();

				if (def != null)
					column.DefaultValue = Types.Convert<string>(def.Value, CultureInfo.InvariantCulture);

				if (property.FindAttribute<VersionAttribute>() != null)
					column.IsVersion = true;
				else
				{
					var maxLength = property.FindAttribute<LengthAttribute>();

					if (maxLength != null)
						column.MaxLength = maxLength.Length;
				}

				var nullable = property.FindAttribute<NullableAttribute>();

				column.IsNullable = nullable != null && nullable.IsNullable;

				var dep = property.FindAttribute<DependencyAttribute>();

				if (dep != null)
				{
					column.DependencyType = dep.Model.Name;
					column.DependencyProperty = dep.Property;
				}

				columns.Add(column);
			}

			if (columns.Count > 0)
				result.Columns.AddRange(columns.OrderBy(f => f.Ordinal).ThenBy(f => f.Name));

			return result;
		}

		private string ResolveColumnName(PropertyInfo property)
		{
			var mapping = property.FindAttribute<NameAttribute>();

			if (mapping != null)
				return mapping.ColumnName;

			return property.Name.ToCamelCase();
		}

		private List<IConnectionString> ResolveConnections(IModelConfiguration configuration)
		{
			using var ctx = new MicroServiceContext(configuration.MicroService());
			var type = configuration.Middleware(ctx);

			if (type == null)
				return null;

			var middleware = ctx.CreateMiddleware<IModelComponent>(type);

			if (middleware == null)
				return null;

			var attributes = middleware.GetType().GetCustomAttributes(true);
			var result = new List<IConnectionString>();

			foreach (var attribute in attributes)
			{
				if (!(attribute is ShardingProviderAttribute provider))
					continue;

				return provider.QueryConnections(ctx);
			}

			if (configuration.Connection == Guid.Empty)
				throw new RuntimeException(nameof(ModelService), $"{SR.ErrModelConnectionNotSet} ({configuration.ComponentName()})", LogCategories.Middleware);

			if (!(Tenant.GetService<IComponentService>().SelectConfiguration(configuration.Connection) is IConnectionConfiguration connection))
				throw new RuntimeException(nameof(ModelService), SR.ErrConnectionNotFound, LogCategories.Middleware);

			var cs = connection.ResolveConnectionString(ctx, ConnectionStringContext.Elevated);

			if (cs != null)
				result.Add(cs);

			return result;
		}
	}
}
