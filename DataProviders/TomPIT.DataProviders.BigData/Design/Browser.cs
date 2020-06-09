using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using TomPIT.Annotations.BigData;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.DataProviders.Design;
using TomPIT.Reflection;

namespace TomPIT.DataProviders.BigData.Design
{
	internal class Browser : SchemaBrowser
	{
		public override ICommandDescriptor CreateCommandDescriptor(string schemaGroup, string groupObject)
		{
			return new CommandDescriptor
			{
				CommandType = CommandType.Text,
				CommandText = groupObject
			};
		}

		public override List<IGroupObject> QueryGroupObjects(IConnectionConfiguration configuration)
		{
			return QueryGroupObjects(configuration, "Partitions");
		}

		public override List<IGroupObject> QueryGroupObjects(IConnectionConfiguration configuration, string schemaGroup)
		{
			var result = new List<IGroupObject>();
			var ms = configuration.MicroService();

			FillPartitions(result, ms);

			var references = Context.Tenant.GetService<IDiscoveryService>().References(ms);

			if (references == null)
				return result;

			foreach (var reference in references.MicroServices)
			{
				if (string.IsNullOrWhiteSpace(reference.MicroService))
					continue;

				var refMs = Context.Tenant.GetService<IMicroServiceService>().Select(reference.MicroService);

				if (refMs == null)
					continue;

				FillPartitions(result, refMs.Token);
			}

			return result;
		}

		private void FillPartitions(List<IGroupObject> items, Guid microService)
		{
			var ms = Context.Tenant.GetService<IMicroServiceService>().Select(microService);
			var partitions = Context.Tenant.GetService<IComponentService>().QueryComponents(microService, ComponentCategories.BigDataPartition);

			foreach (var partition in partitions)
			{
				var identifier = $"{ms.Name}/{partition.Name}";

				items.Add(new GroupObject
				{
					Text = identifier,
					Value = identifier
				});
			}
		}

		public override List<ISchemaParameter> QueryParameters(IConnectionConfiguration configuration, string schemaGroup, string groupObject, DataOperation operation)
		{
			switch (operation)
			{
				case DataOperation.NotSet:
				case DataOperation.Write:
					return QueryWriteParameters(configuration, groupObject);
				case DataOperation.Read:
					return QueryReadParameters(configuration, groupObject);
				default:
					throw new NotSupportedException();
			}
		}

		private List<ISchemaParameter> QueryReadParameters(IConnectionConfiguration configuration, string groupObject)
		{
			var result = new List<ISchemaParameter>
			{
				new SchemaParameter
				{
					DataType = DataType.String,
					Name = "key",
					IsNullable = true
				},

				new SchemaParameter
				{
					DataType = DataType.Date,
					Name = "start",
					IsNullable = true
				},

				new SchemaParameter
				{
					DataType = DataType.Date,
					Name = "end",
					IsNullable = true
				},

				new SchemaParameter
				{
					DataType = DataType.String,
					Name = "query",
					IsNullable = false
				}
			};

			return result;
		}
		private List<ISchemaParameter> QueryWriteParameters(IConnectionConfiguration configuration, string groupObject)
		{
			var schema = QuerySchema(configuration, null, groupObject);
			var result = new List<ISchemaParameter>();

			if (schema == null)
				return result;

			foreach (var field in schema)
			{
				result.Add(new SchemaParameter
				{
					DataType = field.DataType,
					IsNullable = true,
					Name = field.Name
				});
			}

			return result;
		}

		public override List<ISchemaParameter> QueryParameters(IConnectionConfiguration configuration, string groupObject, DataOperation operation)
		{
			return QueryParameters(configuration, null, groupObject, operation);
		}

		public override List<ISchemaField> QuerySchema(IConnectionConfiguration configuration, string schemaGroup, string groupObject)
		{
			var descriptor = ComponentDescriptor.BigDataPartition(Context, groupObject);

			try
			{
				descriptor.Validate();
				var type = descriptor.Configuration.BigDataPartitionType(descriptor.Context);

				if (type == null)
					return new List<ISchemaField>();

				var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var result = new List<ISchemaField>();

				foreach (var property in properties)
				{
					if (property.FindAttribute<BigDataIgnoreAttribute>() != null)
						continue;

					if (!property.PropertyType.IsTypePrimitive())
						continue;

					result.Add(new SchemaField
					{
						DataType = Types.ToDataType(property.PropertyType),
						Name = property.Name
					});
				}

				return result;
			}
			catch
			{
				return new List<ISchemaField>();
			}
		}

		public override List<string> QuerySchemaGroups(IConnectionConfiguration configuration)
		{
			return new List<string>
			{
				"Partitions"
			};
		}
	}
}
