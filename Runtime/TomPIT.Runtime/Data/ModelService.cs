using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

			if (state.IsInitializing)//possible cyclic reference
				return;

			lock (state)
			{
				if (state.IsInitializing)
					return;

				if (!state.Valid)
				{
					state.IsInitializing = true;

					try
					{
						SynchronizeEntity(configuration, state);
					}
					finally
					{
						state.IsInitializing = false;
					}
				}
			}
		}

		private void SynchronizeEntity(IModelConfiguration configuration, EntityState state)
		{
			var schema = CreateSchema(configuration);
			var depSynchronizer = new DepencencySynchronizer(Tenant, schema, configuration);

			depSynchronizer.Synchronize();

			if (schema.Ignore)
			{
				state.Initialized = true;
				state.Valid = true;
				return;
			}

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

			if (provider is IOrmProvider orm)
				orm.Synchronize(cs.Value, schema, procedures);
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

			var dependency = type.FindAttribute<DependencyAttribute>();

			if (dependency != null)
				result.Dependency = dependency.Model.Name;

			foreach (var property in properties)
			{
				if (!property.CanWrite)
					continue;

				if (property.FindAttribute<MappingIgnoreAttribute>() != null)
					continue;

				var column = new ModelSchemaColumn(result)
				{
					Name = ResolveColumnName(property),
					DataType = ResolveDbType(property)
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
			var mapping = property.FindAttribute<MappingAttribute>();

			if (mapping != null)
				return mapping.DataSourceField;

			return property.Name;
		}

		private DbType ResolveDbType(PropertyInfo property)
		{
			var type = property.PropertyType;

			if (type.IsEnum)
				type = Enum.GetUnderlyingType(type);

			if (type == typeof(char) || type == typeof(string))
			{
				if (property.FindAttribute<VersionAttribute>() != null)
					return DbType.Binary;

				return DbType.String;
			}
			else if (type == typeof(byte))
				return DbType.Byte;
			else if (type == typeof(bool))
				return DbType.Boolean;
			else if (type == typeof(DateTime))
			{
				var att = property.FindAttribute<DateAttribute>();

				if (att == null)
					return DbType.DateTime2;

				switch (att.Kind)
				{
					case DateKind.Date:
						return DbType.Date;
					case DateKind.DateTime:
						return DbType.DateTime;
					case DateKind.DateTime2:
						return DbType.DateTime2;
					case DateKind.SmallDateTime:
						return DbType.DateTime;
					case DateKind.Time:
						return DbType.Time;
					default:
						return DbType.DateTime2;
				}
			}
			else if (type == typeof(DateTimeOffset))
				return DbType.DateTimeOffset;
			else if (type == typeof(decimal))
				return DbType.Decimal;
			else if (type == typeof(double))
				return DbType.Double;
			else if (type == typeof(Guid))
				return DbType.Guid;
			else if (type == typeof(short))
				return DbType.Int16;
			else if (type == typeof(int))
				return DbType.Int32;
			else if (type == typeof(long))
				return DbType.Int64;
			else if (type == typeof(sbyte))
				return DbType.SByte;
			else if (type == typeof(float))
				return DbType.Single;
			else if (type == typeof(TimeSpan))
				return DbType.Time;
			else if (type == typeof(ushort))
				return DbType.UInt16;
			else if (type == typeof(uint))
				return DbType.UInt32;
			else if (type == typeof(ulong))
				return DbType.UInt64;
			else if (type == typeof(byte[]))
				return DbType.Binary;
			else
				return DbType.Binary;
		}
	}
}
