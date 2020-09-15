using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
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
			var schemas = CreateSchemas(configuration);

			foreach (var schema in schemas)
			{
				if (schema.Ignore)
					return;

				SynchronizeSchema(configuration, schema);
			}
		}

		private void SynchronizeSchema(IModelConfiguration configuration, ModelSchema schema)
		{
			if (configuration.Connection == Guid.Empty)
				throw new RuntimeException(nameof(ModelService), $"{SR.ErrModelConnectionNotSet} ({configuration.ComponentName()})", LogCategories.Middleware);

			var connection = Tenant.GetService<IComponentService>().SelectConfiguration(configuration.Connection) as IConnectionConfiguration;

			if (connection == null)
				throw new RuntimeException(nameof(ModelService), SR.ErrConnectionNotFound, LogCategories.Middleware);

			var ctx = new MicroServiceContext(configuration.MicroService());
			var cs = connection.ResolveConnectionString(ctx, ConnectionStringContext.Elevated);

			if (cs.DataProvider == Guid.Empty)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotSet, connection.ComponentName()))
				{
					Component = connection.Component,
					EventId = MiddlewareEvents.OpenConnection,
				};
			}

			var provider = Tenant.GetService<IDataProviderService>().Select(cs.DataProvider);

			if (provider == null)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotFound, connection.ComponentName()))
				{
					Component = connection.Component,
					EventId = MiddlewareEvents.OpenConnection
				};
			}

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

			if (provider is IOrmProvider orm)
				orm.Synchronize(cs.Value, schema, procedures);
		}

		private List<ModelSchema> CreateSchemas(IModelConfiguration configuration)
		{
			var ctx = new MicroServiceContext(configuration.MicroService());
			var type = configuration.Middleware(ctx);
			var result = new List<ModelSchema>
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
			}

			var dependency = type.FindAttribute<DependencyAttribute>();

			if (dependency != null)
				result.Dependency = dependency.Model.Name;

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

				if (column.DataType == DbType.Date
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

				result.Columns.Add(column);
			}

			return result;
		}

		private string ResolveColumnName(PropertyInfo property)
		{
			var mapping = property.FindAttribute<NameAttribute>();

			if (mapping != null)
				return mapping.ColumnName;

			return property.Name.ToCamelCase();
		}
	}
}
