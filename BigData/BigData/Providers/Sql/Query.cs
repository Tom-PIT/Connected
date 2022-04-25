using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.BigData;
using TomPIT.BigData.Partitions;
using TomPIT.BigData.Persistence;
using TomPIT.BigData.Transactions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.BigData.Providers.Sql
{
	internal class Query
	{
		private const int MaxConcurrentQuery = 10;
		public Query(IMiddlewareContext context, IPartitionConfiguration configuration, List<QueryParameter> parameters)
		{
			Context = context;
			Configuration = configuration;
			Parameters = parameters;
		}

		public IMiddlewareContext Context { get; }
		public JArray Result { get; private set; }
		private IPartitionConfiguration Configuration { get; }
		public List<QueryParameter> Parameters { get; }
		private Type SchemaType { get; set; }
		private List<IndexParameter> IndexParameters { get; set; }
		private string Key { get; set; }
		private ImmutableList<IPartitionFile> Files { get; set; }
		private DateTime StartTimestamp { get; set; }
		private DateTime EndTimestamp { get; set; }
		public string CommandText { get; set; }
		public void Execute()
		{
			ResolveSchemaType();
			ResolveParameters();
			PrepareCommandText();
			ResolveFiles();
			ProcessQueries();
		}

		private void ProcessQueries()
		{
			Result = new JArray();

			Parallel.ForEach(PrepareProcessors(),
				(f) =>
				{
					var result = f.Execute();

					if (result != null)
					{
						lock (Result)
						{
							foreach (var item in result)
								Result.Add(item);
						}
					}
				});
		}
		private List<QueryProcessor> PrepareProcessors()
		{
			var processors = new List<QueryProcessor>();
			var index = 0;

			for (var i = 0; i < Math.Min(MaxConcurrentQuery, Files.Count); i++)
				processors.Add(new QueryProcessor(this));

			foreach (var file in Files)
			{
				if (index >= MaxConcurrentQuery - 1)
					index = 0;

				processors[index].Files.Add(file);
			}

			return processors;
		}

		private void ResolveParameters()
		{
			var properties = SchemaType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			IndexParameters = new List<IndexParameter>();

			var tsParameter = Parameters.FirstOrDefault(f => string.Compare(Merger.TimestampColumn, f.Name, true) == 0);

			if (tsParameter != null)
				SetTimestampValues(tsParameter.Value);

			foreach (var property in properties)
			{
				var parameter = Parameters.FirstOrDefault(f => string.Compare(f.Name, property.Name, true) == 0);

				if (parameter == null)
					continue;

				if (string.Compare(Merger.TimestampColumn, property.Name, true) == 0)
					continue;

				var partitionKey = property.FindAttribute<BigDataPartitionKeyAttribute>();

				if (partitionKey != null)
					Key = Types.Convert<string>(parameter.Value);

				if (property.FindAttribute<BigDataIndexAttribute>() == null)
					continue;

				var value = parameter.Value;

				if (value == null || string.IsNullOrWhiteSpace(parameter.Value.ToString()))
					continue;

				if (value.GetType().IsCollection())
				{
					var list = value as IList;

					if (list.Count == 1)
					{
						IndexParameters.Add(new IndexDiscreteParameter
						{
							Name = property.Name,
							Value = list[0],
							ValueType = property.PropertyType
						});
					}
					else if (list.Count == 2)
					{
						IndexParameters.Add(new IndexRangeParameter
						{
							Name = property.Name,
							StartValue = list[0],
							EndValue = list[1],
							ValueType = property.PropertyType
						});
					}
					else
					{
						var arrayParameter = new IndexArrayParameter
						{
							Name = property.Name,
							ValueType = property.PropertyType
						};

						foreach (var i in list)
							arrayParameter.Values.Add(i);

						IndexParameters.Add(arrayParameter);
					}
				}
				else
				{
					IndexParameters.Add(new IndexDiscreteParameter
					{
						Name = property.Name,
						Value = value,
						ValueType = property.PropertyType
					});
				}
			}
		}

		private void SetTimestampValues(object value)
		{
			if (value == null)
				return;

			if (value.GetType().IsCollection())
			{
				var timestamps = value as IList;

				StartTimestamp = Types.Convert<DateTime>(timestamps[0]);
				EndTimestamp = Types.Convert<DateTime>(timestamps[1]);
			}
			else
				StartTimestamp = Types.Convert<DateTime>(value);
		}

		private void PrepareCommandText()
		{
			var query = Parameters.FirstOrDefault(f => string.Compare(f.Name, "Query", true) == 0);
			var queryName = string.Empty;

			if (query != null)
			{
				queryName = Types.Convert<string>(query.Value);

				Parameters.Remove(query);
			}

			if (Configuration.Queries.Count == 0)
				throw new RuntimeException(nameof(Query), $"{SR.ErrNoQuery} ({Configuration.ComponentName()})", LogCategories.BigData);

			var config = string.IsNullOrWhiteSpace(queryName)
				? Configuration.Queries.First()
				: Configuration.Queries.FirstOrDefault(f => string.Compare(f.Name, queryName, true) == 0);

			if (config == null)
				throw new RuntimeException(nameof(Query), $"{SR.ErrQueryNotFound} {Configuration.ComponentName()}/{queryName}", LogCategories.BigData);

			CommandText = Context.Tenant.GetService<IComponentService>().SelectText(Configuration.MicroService(), config);
		}

		private void ResolveSchemaType()
		{
			using var ctx = new MicroServiceContext(Configuration.MicroService());
			SchemaType = Configuration.BigDataPartitionType(ctx);

			if (SchemaType == null)
				throw new RuntimeException(nameof(SqlPersistenceService), $"{SR.ErrCannotResolveComponentType} ({Configuration.ComponentName()})", LogCategories.BigData);
		}

		private void ResolveFiles()
		{
			Files = Context.Tenant.GetService<IPartitionService>().QueryFiles(Configuration.Component, ResolveTimezone(), Key, StartTimestamp, EndTimestamp, IndexParameters);
		}

		private Guid ResolveTimezone()
		{
			if (!Configuration.SupportsTimezone())
				return Guid.Empty;

			if(MiddlewareDescriptor.Current.User is IUser user && !string.IsNullOrWhiteSpace(user.TimeZone))
			{
				if (Tenant.GetService<ITimezoneService>().Select(user.TimeZone) is ITimezone timezone)
					return timezone.Token;
			}

			return Guid.Empty;
		}
	}
}
