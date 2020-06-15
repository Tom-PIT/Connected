using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Reflection;
using TomPIT.Annotations;
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
		private Lazy<ConcurrentDictionary<Guid, EntityState>> _state = new Lazy<ConcurrentDictionary<Guid, EntityState>>();

		public ModelService(ITenant tenant) : base(tenant)
		{
			Tenant.GetService<IComponentService>().ComponentChanged += OnComponentChanged;
			Tenant.GetService<IComponentService>().ConfigurationChanged += OnConfigurationChanged;
		}

		private void OnConfigurationChanged(ITenant sender, ConfigurationEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Model, true) == 0)
				InvalidateEntity(e.Component);
		}

		private void OnComponentChanged(ITenant sender, ComponentEventArgs e)
		{
			if (string.Compare(e.Category, ComponentCategories.Model, true) == 0)
				InvalidateEntity(e.Component);
		}

		private void InvalidateEntity(Guid configuration)
		{
			if (!State.TryGetValue(configuration, out EntityState state))
				return;

			InvalidateState(state);
		}

		public void InvalidateEntity(IModelConfiguration configuration)
		{
			InvalidateState(EnsureState(configuration));
		}

		private void InvalidateState(EntityState state)
		{
			lock (state)
			{
				state.Valid = false;

				foreach (var operation in state.Operations)
				{
					operation.Valid = false;
					operation.Text = null;
				}
			}
		}

		public void SynchronizeEntity(IModelConfiguration configuration)
		{
			var state = EnsureState(configuration);

			lock (state)
			{
				if (!state.Valid)
					SynchronizeEntity(configuration, state);
			}
		}

		private void SynchronizeEntity(IModelConfiguration configuration, EntityState state)
		{
			var schema = CreateSchema(configuration);

			if (state.Initialized && schema.Equals(state.Schema))
			{
				state.Valid = true;

				if (state.Operations.Count == configuration.Operations.Count)
				{
					var ms = configuration.MicroService();

					for (var i = 0; i < configuration.Operations.Count; i++)
					{
						var stateOp = new ModelOperationSchema
						{
							Text = Tenant.GetService<IComponentService>().SelectText(ms, configuration.Operations[i])
						};

						if (!stateOp.Equals(state.Operations[i]))
						{
							state.Valid = false;
							break;
						}
					}

					if (state.Valid)
						return;
				}
			}

			SynchronizeSchema(configuration, schema);

			state.Schema = schema;
			state.Valid = true;
			state.Initialized = true;
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

			if (provider is IDeployDataProvider deploy)
				deploy.Synchronize(cs.Value, schema, procedures);
		}

		private EntityState EnsureState(IModelConfiguration configuration)
		{
			if (!State.TryGetValue(configuration.Component, out EntityState existing))
			{
				existing = new EntityState
				{
					Valid = false,
				};

				existing.Schema = CreateSchema(configuration);
				var ms = configuration.MicroService();

				foreach (var operation in configuration.Operations)
				{
					existing.Operations.Add(new OperationState
					{
						Id = operation.Id,
						Initialized = false,
						Name = operation.Name,
						Text = Tenant.GetService<IComponentService>().SelectText(ms, operation)
					});
				}
			};

			if (!State.TryAdd(configuration.Component, existing))
			{
				if (!State.TryGetValue(configuration.Component, out existing))
					throw new RuntimeException(SR.ErrCannotGetEntityState);
			}

			return existing;
		}

		private ConcurrentDictionary<Guid, EntityState> State => _state.Value;

		private ModelSchema CreateSchema(IModelConfiguration configuration)
		{
			var type = configuration.ModelType(new MicroServiceContext(configuration.MicroService()));
			var properties = ConfigurationExtensions.GetMiddlewareProperties(type, true);
			var result = new ModelSchema();

			var schema = type.FindAttribute<SchemaAttribute>();

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

			foreach (var property in properties)
			{
				if (!property.CanWrite)
					continue;

				if (property.FindAttribute<MappingIgnoreAttribute>() != null)
					continue;

				var column = new ModelSchemaColumn
				{
					Name = ResolveColumnName(property),
					DataType = ResolveDbType(property)
				};

				var pk = property.FindAttribute<PrimaryKeyAttribute>();

				if (pk != null)
				{
					column.IsPrimaryKey = true;
					column.IsIdentity = pk.Identity;
				}

				var idx = property.FindAttribute<IndexAttribute>();

				if (idx != null)
				{
					column.IsIndex = true;
					column.IsUnique = idx.Unique;
				}

				var def = property.FindAttribute<DefaultValueAttribute>();

				if (def != null)
					column.DefaultValue = Types.Convert<string>(def.Value, CultureInfo.InvariantCulture);

				var maxLength = property.FindAttribute<MaxLengthAttribute>();

				if (maxLength != null)
					column.MaxLength = maxLength.Length;

				var nullable = property.FindAttribute<NullableAttribute>();

				column.IsNullable = nullable != null && nullable.IsNullable;

				var dependency = property.FindAttribute<DependencyAttribute>();

				if (dependency != null)
					column.Dependency = dependency.Model;

				result.Columns.Add(column);
			}

			return result;
		}

		private string ResolveColumnName(PropertyInfo property)
		{
			var mapping = property.FindAttribute<MappingAttribute>();

			if (mapping != null)
				return mapping.DataSourceField;

			return property.Name;
		}

		private DbType ResolveDbType(PropertyInfo property)
		{
			if (property.PropertyType == typeof(char) || property.PropertyType == typeof(string))
				return DbType.String;
			else if (property.PropertyType == typeof(byte))
				return DbType.Byte;
			else if (property.PropertyType == typeof(bool))
				return DbType.Boolean;
			else if (property.PropertyType == typeof(DateTime))
				return DbType.DateTime2;
			else if (property.PropertyType == typeof(DateTimeOffset))
				return DbType.DateTimeOffset;
			else if (property.PropertyType == typeof(decimal))
				return DbType.Decimal;
			else if (property.PropertyType == typeof(double))
				return DbType.Double;
			else if (property.PropertyType == typeof(Guid))
				return DbType.Guid;
			else if (property.PropertyType == typeof(short))
				return DbType.Int16;
			else if (property.PropertyType == typeof(int))
				return DbType.Int32;
			else if (property.PropertyType == typeof(long))
				return DbType.Int64;
			else if (property.PropertyType == typeof(sbyte))
				return DbType.SByte;
			else if (property.PropertyType == typeof(float))
				return DbType.Single;
			else if (property.PropertyType == typeof(TimeSpan))
				return DbType.Time;
			else if (property.PropertyType == typeof(ushort))
				return DbType.UInt16;
			else if (property.PropertyType == typeof(uint))
				return DbType.UInt32;
			else if (property.PropertyType == typeof(ulong))
				return DbType.UInt64;
			else if (property.PropertyType == typeof(byte[]))
				return DbType.Binary;
			else
				return DbType.Object;
		}
	}
}
