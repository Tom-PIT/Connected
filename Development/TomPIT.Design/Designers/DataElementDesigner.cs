using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	public abstract class DataElementDesigner : DomDesigner<ReflectionElement>
	{
		private List<ComponentModel.IComponent> _connections = null;
		private ISchemaBrowser _browser = null;
		private IDataProvider _schemaProvider = null;
		private List<string> _repositories = null;
		private Guid _connection = Guid.Empty;
		private List<ISchemaParameter> _parameters = null;
		private IConnection _connectionConfiguration = null;

		public DataElementDesigner(ComponentElement element) : base(element)
		{
		}

		protected override void OnCreateToolbar(IDesignerToolbar toolbar)
		{
			var action = new ToolbarAction(Environment)
			{

				Glyph = "fal fa-upload",
				Id = "import",
				Text = "Import"
			};


			toolbar.Items.Add(action);
		}

		public List<ComponentModel.IComponent> Connections
		{
			get
			{
				if (_connections == null)
					_connections = Connection.GetService<IComponentService>().QueryComponents(Element.MicroService(), "Connection");

				return _connections;
			}
		}

		protected string ObjectType { get; private set; }
		protected string Object { get; private set; }

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			Initialize(data);

			if (string.Compare(action, "objectTypes", true) == 0)
				return ActionObjectTypes();

			else if (string.Compare(action, "objects", true) == 0)
				return ActionObjects();
			else if (string.Compare(action, "object", true) == 0)
				return ActionObject();
			else if (string.Compare(action, "import", true) == 0)
				return ActionImport();
			else
				throw IdeException.DesignerActionNotSupported(this, IdeEvents.DesignerAction, action);
		}

		private IDesignerActionResult ActionObjectTypes()
		{
			if (Connection == null || Repositories == null)
				return Result.EmptyResult(this);

			var r = new SelectItems();

			foreach (var i in Repositories)
				r.Add(i, i);

			return Result.JsonResult(this, r);
		}

		private IDesignerActionResult ActionObjects()
		{
			if (string.IsNullOrWhiteSpace(ObjectType) || Browser == null)
				return Result.EmptyResult(this);

			var ds = Browser.QueryGroupObjects(ConnectionConfiguration, ObjectType);
			var r = new SelectItems();

			foreach (var i in ds)
				r.Add(i, i);

			return Result.JsonResult(this, r);
		}

		protected abstract IDesignerActionResult ActionObject();
		protected abstract IDesignerActionResult ActionImport();
		protected string SelectedParameters = string.Empty;

		private void Initialize(JObject data)
		{
			_connection = data.Optional("connection", Guid.Empty);

			if (_connection == Guid.Empty)
				return;

			DataConnection = Connections.FirstOrDefault(f => f.Token == _connection);

			ObjectType = data.Optional("objectType", string.Empty);
			Object = data.Optional("object", string.Empty);
			SelectedParameters = data.Optional("parameters", string.Empty);

			OnInitialize(data);
		}

		protected virtual void OnInitialize(JObject data)
		{

		}

		protected ComponentModel.IComponent DataConnection { get; private set; }
		protected IConnection ConnectionConfiguration
		{
			get
			{
				if (_connectionConfiguration == null)
					_connectionConfiguration = Connection.GetService<IComponentService>().SelectConfiguration(DataConnection.Token) as IConnection;

				return _connectionConfiguration;
			}
		}

		protected IDataProvider DataProvider
		{
			get
			{
				if (_schemaProvider == null && Connection != null)
				{
					_schemaProvider = Connection.GetService<IDataProviderService>().Select(ConnectionConfiguration.DataProvider);

					if (_schemaProvider == null)
						throw IdeException.DataProviderNotFound(this, IdeEvents.DesignerAction, DataConnection.Name);
				}

				return _schemaProvider;
			}
		}

		protected ISchemaBrowser Browser
		{
			get
			{
				if (_browser == null && DataProvider != null)
				{
					var att = DataProvider.GetType().FindAttribute<SchemaBrowserAttribute>();

					if (att != null)
					{
						_browser = att.Type == null
							? Types.GetType(att.TypeName).CreateInstance<ISchemaBrowser>()
							: att.Type.CreateInstance<ISchemaBrowser>();
					}
				}

				return _browser;
			}
		}

		public List<string> Repositories
		{
			get
			{
				if (_repositories == null && Connection != null)
					_repositories = Browser.QuerySchemaGroups(ConnectionConfiguration);

				return _repositories;
			}
		}

		public List<ISchemaParameter> Parameters
		{
			get
			{
				if (_parameters == null && Browser != null
					&& !string.IsNullOrWhiteSpace(ObjectType) && !string.IsNullOrWhiteSpace(Object))
					_parameters = Browser.QueryParameters(ConnectionConfiguration, ObjectType, Object);

				if (_parameters == null)
					_parameters = new List<ISchemaParameter>();

				return _parameters;
			}
		}
	}
}
