using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.Annotations.Models;
using TomPIT.Data.Schema;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Data.Storage;
internal class StorageSynchronizationService : IStorageSynchronizationService
{
	public async Task Synchronize(List<Type>? entities)
	{
		if (entities is null || !entities.Any())
			return;

		using var ctx = new MiddlewareContext();
		var providers = await Tenant.GetService<IMiddlewareService>().Query<ISchemaMiddleware>(ctx);

		foreach (var entity in entities)
		{
			if (!IsPersistent(entity))
				continue;

			Tenant.LogInfo($"Synchronizing entity '{entity.Name}");

			var synchronized = false;
			var schema = CreateSchema(entity);

			if (schema.Ignore)
				continue;

			foreach (var middleware in providers)
			{
				/*
				 * We are looking for the first middleware which returns true,
				 * which means it supports entity synchronization.
				 */
				if (!await middleware.IsEntitySupported(entity))
					continue;
				/*
				 * Note that sharding synchronization will be handled by the middleware.
				 */
				await middleware.Synchronize(entity, schema);

				synchronized = true;
				break;
			}
			/*
			 * We should notify the environment that entity is no synchronized.
			 * Maybe we should throw the exception here because unsynchronized
			 * entities could cause system instabillity.
			 */
			if (!synchronized)
				Tenant.LogWarning($"No middleware synchronized the entity ({entity.Name}).");
		}
	}
	/// <summary>
	/// Determines if the entity supports persistence. Virtual entities does not support persistence which
	/// means they don't have physical storage.
	/// </summary>
	/// <param name="entityType">The type of the entity to check for persistence.</param>
	/// <returns><c>true</c> if the entity supports persistence, <c>false</c> otherwise.</returns>
	private static bool IsPersistent(Type entityType)
	{
		var persistence = entityType.GetCustomAttribute<PersistenceAttribute>();

		return persistence is null || persistence.Persistence.HasFlag(ColumnPersistence.Write);
	}

	private static ISchema CreateSchema(Type type)
	{
		var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		var att = ResolveSchemaAttribute(type);

		var result = new EntitySchema
		{
			Name = att.Name,
			Schema = att.Schema
		};

		var columns = new List<SchemaColumn>();

		foreach (var property in properties)
		{
			if (!property.CanWrite)
				continue;

			if (property.FindAttribute<PersistenceAttribute>() is PersistenceAttribute pa && pa.IsVirtual)
				continue;

			var column = new SchemaColumn(result, property)
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
				column.Index = idx.Group;
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
				var dateAtt = property.FindAttribute<DateAttribute>();

				if (dateAtt is not null)
				{
					column.DateKind = dateAtt.Kind;
					column.DatePrecision = dateAtt.Precision;
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

				if (bin is not null)
					column.BinaryKind = bin.Kind;
			}
			else if (column.DataType == DbType.String
				 || column.DataType == DbType.AnsiString
				 || column.DataType == DbType.AnsiStringFixedLength
				 || column.DataType == DbType.StringFixedLength)
			{
				column.MaxLength = 50;
			}

			ParseDefaultValue(column, property);

			if (property.FindAttribute<ETagAttribute>() is not null)
				column.IsVersion = true;
			else
			{
				var maxLength = property.FindAttribute<LengthAttribute>();

				if (maxLength is not null)
					column.MaxLength = maxLength.Length;
			}

			var nullable = property.FindAttribute<NullableAttribute>();

			if (nullable is null)
				column.IsNullable = property.PropertyType.IsNullableType();
			else
				column.IsNullable = nullable.IsNullable;

			columns.Add(column);
		}

		if (columns.Any())
			result.Columns.AddRange(columns.OrderBy(f => f.Ordinal).ThenBy(f => f.Name));

		return result;
	}

	private static SchemaAttribute ResolveSchemaAttribute(Type type)
	{
		var att = type.GetCustomAttribute<SchemaAttribute>() ?? new TableAttribute();

		if (string.IsNullOrWhiteSpace(att.Name))
			att.Name = type.Name.ToCamelCase();

		if (string.IsNullOrEmpty(att.Schema))
			att.Schema = SchemaAttribute.DefaultSchema;

		return att;
	}

	private static string ResolveColumnName(PropertyInfo property)
	{
		if (property.FindAttribute<MemberAttribute>() is not MemberAttribute mapping || string.IsNullOrWhiteSpace(mapping.Member))
			return property.Name.ToCamelCase();

		return mapping.Member;
	}

	private static void ParseDefaultValue(SchemaColumn column, PropertyInfo property)
	{
		if (property.FindAttribute<DefaultAttribute>() is not DefaultAttribute def)
			return;

		var value = def.Value;

		if (def.Value is not null && def.Value.GetType().IsEnum)
			value = Types.Convert(def.Value, def.Value.GetType().GetEnumUnderlyingType());

		column.DefaultValue = Types.Convert<string>(value, CultureInfo.InvariantCulture);
	}
}
