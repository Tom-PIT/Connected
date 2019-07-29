using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.BigData.Services;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Data
{
	internal class PartitionSchema
	{
		private List<PartitionSchemaField> _fields = null;

		private static readonly List<Type> SupportedTypes = new List<Type>
		{
			typeof(byte),
			typeof(short),
			typeof(int),
			typeof(long),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(string),
			typeof(char),
			typeof(DateTime),
			typeof(bool),
			typeof(Guid)
		};

		public PartitionSchema(IPartitionConfiguration configuration)
		{
			Configuration = configuration;

			Discover();
		}

		private IPartitionConfiguration Configuration { get; }

		public string PartitionKeyField { get; private set; }
		public string KeyField { get; private set; }
		public List<PartitionSchemaField> Fields
		{
			get
			{
				if (_fields == null)
					_fields = new List<PartitionSchemaField>();

				return _fields;
			}
		}

		private void Discover()
		{
			var type = Instance.GetService<ICompilerService>().ResolveType(((IConfiguration)Configuration).MicroService(Instance.Connection), Configuration, Configuration.ComponentName(Instance.Connection));

			if (type == null)
				return;

			var handler = type.GetInterface(typeof(IPartitionHandler<>).FullName);
			var entityType = handler.GetGenericArguments()[0];
			var properties = entityType.GetProperties();

			foreach (var property in properties)
			{
				if (!property.CanRead || !property.GetMethod.IsPublic)
					continue;

				if (string.Compare(property.Name, Merger.IdColumn, true) == 0)
					continue;

				if (property.FindAttribute<BigDataPartitionKeyAttribute>() != null)
				{
					PartitionKeyField = property.Name;
					continue;
				}

				var ignore = property.FindAttribute<BigDataIngoreAttribute>();

				if (ignore != null)
					continue;

				if (!SupportedTypes.Contains(property.PropertyType))
					continue;

				var field = ResolveField(property);

				if (field == null)
					continue;

				field.Name = property.Name;
				field.Key = IsKey(property);
				field.Index = IsIndex(property);

				Fields.Add(field);

				if (field.Key)
					KeyField = property.Name;
			}

			if (Fields.FirstOrDefault(f => string.Compare(f.Name, Merger.TimestampColumn, true) == 0) == null)
			{
				Fields.Add(new PartitionSchemaDateField
				{
					Index=false,
					Key=false,
					Name=Merger.TimestampColumn,
					Type=typeof(DateTime)
				});
			}
		}

		private bool IsIndex(PropertyInfo property)
		{
			return property.FindAttribute<BigDataIndexAttribute>() != null;
		}

		private bool IsKey(PropertyInfo property)
		{
			return property.FindAttribute<BigDataKeyAttribute>() != null;
		}

		private PartitionSchemaField ResolveField(PropertyInfo property)
		{
			if (property.PropertyType == typeof(char))
			{
				return new PartitionSchemaStringField
				{
					Length = 1,
					Type=typeof(string)
				};
			}
			else if (property.PropertyType == typeof(string))
			{
				return new PartitionSchemaStringField
				{
					Length = ResolveLength(property),
					Type = typeof(string)
				};
			}
			else if (property.PropertyType == typeof(Guid))
			{
				return new PartitionSchemaStringField
				{
					Length = 36,
					Type = typeof(string)
				};
			}
			else if (property.PropertyType == typeof(byte)
				|| property.PropertyType == typeof(short)
				|| property.PropertyType == typeof(int)
				|| property.PropertyType == typeof(long)
				|| property.PropertyType == typeof(float)
				|| property.PropertyType == typeof(double)
				|| property.PropertyType == typeof(decimal))
			{
				return new PartitionSchemaNumberField
				{
					Type = property.PropertyType,
				};
			}
			else if (property.PropertyType == typeof(DateTime))
			{
				return new PartitionSchemaDateField
				{
					Type=typeof(DateTime)
				};
			}

			return null;
		}

		private int ResolveLength(PropertyInfo property)
		{
			var att = property.FindAttribute<BigDataLengthAttribute>();

			return att == null ? 50 : att.Length;
		}
	}
}
