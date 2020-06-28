using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Models;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Data
{
	public abstract class ModelMiddleware<T> : MiddlewareComponent, IModelMiddleware<T>
	{
		private IComponent _component = null;
		private IModelConfiguration _configuration = null;
		public int Execute([CIP(CIP.ModelExecuteOperationProvider)]string operation)
		{
			return Execute(operation, null);
		}

		public int Execute([CIP(CIP.ModelExecuteOperationProvider)]string operation, [CIP(CIP.ModelOperationParametersProvider)]object e)
		{
			var op = ResolveOperation(operation);
			var con = OpenConnection();
			var descriptor = CreateDescriptor(op, con);

			var w = Context.OpenWriter(con, descriptor.CommandText);

			w.CommandType = descriptor.Type == CommandTextType.Procedure ? CommandType.StoredProcedure : CommandType.Text;

			BindParameters(w, e, descriptor);

			var recordsAffected = w.Execute();

			if (recordsAffected == 0 && Concurrency == ConcurrencyMode.Enabled)
				throw new ConcurrencyException(GetType(), operation);

			BindReturnValues(w, e);

			return recordsAffected;
		}

		public List<T> Query([CIP(CIP.ModelQueryOperationProvider)]string operation)
		{
			return Query(operation, null);
		}

		public List<R> Query<R>([CIP(CIP.ModelQueryOperationProvider)]string operation)
		{
			return Query<R>(operation, null);
		}

		public List<T> Query([CIP(CIP.ModelQueryOperationProvider)]string operation, [CIP(CIP.ModelOperationParametersProvider)]object e)
		{
			return Query<T>(operation, e);
		}

		public List<R> Query<R>([CIP(CIP.ModelQueryOperationProvider)]string operation, [CIP(CIP.ModelOperationParametersProvider)]object e)
		{
			var op = ResolveOperation(operation);
			var con = OpenConnection();
			var descriptor = CreateDescriptor(op, con);

			var r = Context.OpenReader<R>(con, descriptor.CommandText);

			r.CommandType = descriptor.Type == CommandTextType.Procedure ? CommandType.StoredProcedure : CommandType.Text;

			BindParameters(r, e, descriptor);

			return r.Query();
		}

		public T Select([CIP(CIP.ModelQueryOperationProvider)]string operation)
		{
			return Select(operation, null);
		}

		public R Select<R>([CIP(CIP.ModelQueryOperationProvider)]string operation)
		{
			return Select<R>(operation, null);
		}

		public T Select([CIP(CIP.ModelQueryOperationProvider)]string operation, [CIP(CIP.ModelOperationParametersProvider)]object e)
		{
			return Select<T>(operation, e);
		}

		public R Select<R>([CIP(CIP.ModelQueryOperationProvider)]string operation, [CIP(CIP.ModelOperationParametersProvider)]object e)
		{
			var op = ResolveOperation(operation);
			var con = OpenConnection();
			var descriptor = CreateDescriptor(op, con);

			var r = Context.OpenReader<R>(con, descriptor.CommandText);

			r.CommandType = descriptor.Type == CommandTextType.Procedure ? CommandType.StoredProcedure : CommandType.Text;

			BindParameters(r, e, descriptor);

			return r.Select();
		}

		private IComponent Component
		{
			get
			{
				if (_component == null)
					_component = Context.Tenant.GetService<ICompilerService>().ResolveComponent(this);

				return _component;
			}
		}

		private IModelConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = Context.Tenant.GetService<IComponentService>().SelectConfiguration(Component.Token) as IModelConfiguration;

				return _configuration;
			}
		}

		private IModelOperation ResolveOperation(string operation)
		{
			SyncEntity();

			var op = Configuration.Operations.FirstOrDefault(f => string.Compare(f.Name, operation, true) == 0);

			if (op == null)
				throw Context.Services.Diagnostic.Exception($"{SR.ErrModelOperationNotFound} ({Component.Name}/{operation})");

			return op;
		}

		private void SyncEntity()
		{
			Context.Tenant.GetService<IModelService>().SynchronizeEntity(Configuration);
		}

		private IDataConnection OpenConnection()
		{
			if (Configuration.Connection == Guid.Empty)
				throw new RuntimeException(nameof(ModelMiddleware<T>), $"{SR.ErrModelConnectionNotSet} ({Configuration.ComponentName()})", LogCategories.Middleware);

			var connection = Context.Tenant.GetService<IComponentService>().SelectComponent(Configuration.Connection);

			if (connection == null)
				throw new RuntimeException(nameof(ModelMiddleware<T>), SR.ErrConnectionNotFound, LogCategories.Middleware);

			var ms = Context.Tenant.GetService<IMicroServiceService>().Select(Configuration.MicroService());
			//TODO: implement option to set ConnectionBehavior
			return Context.OpenConnection($"{ms.Name}/{connection.Name}", ConnectionBehavior.Shared, null);
		}

		private ICommandTextDescriptor CreateDescriptor(IModelOperation operation, IDataConnection connection)
		{
			var text = Context.Tenant.GetService<IComponentService>().SelectText(Configuration.MicroService(), operation);

			if (string.IsNullOrWhiteSpace(text))
				throw new RuntimeException(nameof(ModelMiddleware<T>), $"{SR.ErrCommandTextNotSet} ({Configuration.ComponentName()}/{operation.Name})", LogCategories.Middleware);

			var parser = connection.Parser;

			if (parser == null)
				throw new RuntimeException(nameof(ModelMiddleware<T>), $"{SR.DataProviderParserNull} ({Configuration.ComponentName()}/{operation.Name})", LogCategories.Middleware);

			return parser.Parse(text);
		}

		private void BindParameters(IDataCommand command, object e, ICommandTextDescriptor descriptor)
		{
			foreach (var parameter in command.Parameters)
				command.SetParameter(parameter.Name, null);

			if (e == null)
				return;

			var properties = e.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var parameter in descriptor.Parameters)
			{
				PropertyInfo property = null;

				foreach (var prop in properties)
				{
					var att = prop.FindAttribute<ParameterMappingAttribute>();

					if (att != null && string.Compare(parameter.Name, att.Name, true) == 0)
					{
						property = prop;
						break;
					}
				}

				if (property == null)
					property = properties.FirstOrDefault(f => string.Compare(f.Name, parameter.Name, false) == 0);

				if (property == null)
					property = properties.FirstOrDefault(f => string.Compare(f.Name, parameter.Name, true) == 0);

				if (property == null)
				{
					var candidates = new List<string>
					{
						parameter.Name.Replace("@", "")
					};

					foreach (var prop in properties)
					{
						foreach (var candidate in candidates)
						{
							if (string.Compare(candidate, prop.Name, true) == 0)
							{
								property = prop;
								break;
							}
						}

						if (property != null)
							break;
					}
				}

				if (property != null)
				{
					var value = property.GetValue(e);
					var nullable = property.FindAttribute<NullableAttribute>() != null;
					var version = property.FindAttribute<VersionAttribute>();

					if (version != null)
						value = (byte[])Version.Parse(value);

					command.SetParameter(parameter.Name, value, nullable);
				}
			}

			if (command is IDataWriter writer)
			{
				foreach (var property in properties)
				{
					var att = property.FindAttribute<ReturnValueAttribute>();

					if (att != null)
					{
						var parameter = writer.SetReturnValueParameter(property.Name);

						parameter.Type = property.PropertyType;
					}
				}
			}
		}

		public T CreateEntity(object instance)
		{
			return CreateEntity<T>(instance);
		}

		public R CreateEntity<R>(object instance)
		{
			var result = (R)typeof(R).CreateInstance();

			Serializer.Populate(instance, result);

			return result;
		}

		private void BindReturnValues(IDataWriter w, object e)
		{
			if (e == null)
				return;

			List<PropertyInfo> properties = null;

			foreach (var parameter in w.Parameters)
			{
				if (parameter.Direction != ParameterDirection.ReturnValue)
					continue;

				if (parameter.Value == DBNull.Value)
					continue;

				if (properties == null)
				{
					properties = new List<PropertyInfo>();

					var all = e.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

					foreach (var prop in all)
					{
						if (prop.FindAttribute<ReturnValueAttribute>() != null)
							properties.Add(prop);
					}
				}

				PropertyInfo property = null;

				foreach (var prop in properties)
				{
					var name = prop.FindAttribute<ParameterMappingAttribute>();

					if (name != null)
					{
						if (string.Compare(name.Name, parameter.Name, true) == 0)
							property = prop;

						break;
					}
				}

				if (property == null)
				{
					foreach (var prop in properties)
					{
						if (string.Compare(prop.Name, parameter.Name, false) == 0)
						{
							property = prop;
							break;
						}
					}
				}

				if (property == null)
				{
					foreach (var prop in properties)
					{
						if (string.Compare(prop.Name, parameter.Name, true) == 0)
						{
							property = prop;
							break;
						}
					}
				}

				if (property == null)
				{
					var candidates = new List<string>
					{
						parameter.Name.Replace("@", string.Empty)
					};

					foreach (var prop in properties)
					{
						foreach (var candidate in candidates)
						{
							if (string.Compare(prop.Name, candidate, true) == 0)
							{
								property = prop;
								break;
							}
						}

						if (property != null)
							break;
					}
				}

				if (property != null)
				{
					var existingValue = property.GetValue(e);
					var overwriteAtt = property.FindAttribute<ReturnValueAttribute>();

					switch (overwriteAtt.ValueBehavior)
					{
						case PropertyValueBehavior.OverwriteDefault:
							var defaultValue = property.PropertyType.GetDefaultValue();

							if (Types.Compare(existingValue, defaultValue))
								property.SetValue(e, parameter.Value);
							break;
						case PropertyValueBehavior.AlwaysOverwrite:
							property.SetValue(e, parameter.Value);
							break;
					}
				}
			}
		}

		public T Merge(T entity, object instance)
		{
			if (instance == null)
				return entity;

			return OnMerge(entity, instance);
		}

		protected virtual T OnMerge(T entity, object instance)
		{
			var browser = new PropertyBrowser(instance)
			{
				Mode = PropertyBrowserMode.Reflection,
				ReadWrite = false,
				Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
			};

			var properties = browser.Browse();

			foreach (var property in properties)
			{
				if (property.IsCollection())
					continue;

				if (property.PropertyType.IsTypePrimitive())
				{
					var target = entity.GetType().GetProperty(property.Name);

					if (target == null || !target.CanWrite)
						continue;

					var pk = target.FindAttribute<PrimaryKeyAttribute>();

					if (pk != null)
						continue;

					try
					{
						var value = property.GetValue(instance);

						if (Types.TryConvert(value, out object converted, target.PropertyType))
							target.SetValue(entity, converted);
					}
					catch { }
				}
			}

			return entity;
		}

		public ConcurrencyMode Concurrency { get; set; } = ConcurrencyMode.Enabled;
	}
}
