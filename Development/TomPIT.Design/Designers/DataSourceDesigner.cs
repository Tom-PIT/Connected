using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	public abstract class DataSourceDesigner : DataElementDesigner
	{
		private List<ISchemaField> _fields = null;
		private string _selectedFields = string.Empty;

		public DataSourceDesigner(IEnvironment environment, ComponentElement element) : base(environment, element)
		{
		}

		public override string View { get { return "~/Views/Ide/Designers/DataSource.cshtml"; } }

		public override object ViewModel
		{
			get { return this; }
		}

		protected override IDesignerActionResult ActionObject()
		{
			if (string.IsNullOrWhiteSpace(ObjectType)
				|| string.IsNullOrWhiteSpace(Object) || Browser == null)
				return Result.EmptyResult(this);

			var vb = new Dictionary<string, object>(){
					{ "fields",Fields },
					{"parameters", Parameters }
				};

			return Result.JsonResult(this, vb);
		}

		public IDataSource DataSource { get { return Owner.Component as IDataSource; } }

		protected abstract void SetAttributes(Guid connection, string commandText, CommandType commandType);
		protected abstract IBoundField CreateField(string name, DataType dataType);
		protected abstract ComponentModel.Data.IDataParameter CreateParameter(string name, DataType dataType, bool isNullable);

		protected override IDesignerActionResult ActionImport()
		{
			if (Connection == null)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "connection");

			if (string.IsNullOrWhiteSpace(ObjectType))
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "objectType");

			if (string.IsNullOrWhiteSpace(Object))
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "object");

			if (string.IsNullOrWhiteSpace(_selectedFields))
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "fields");

			var command = Browser.CreateCommandDescriptor(ObjectType, Object);

			SetAttributes(DataConnection.Token, command.CommandText, command.CommandType);

			DataSource.Fields.Clear();

			var tokens = _selectedFields.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var i in tokens)
			{
				var field = Fields.FirstOrDefault(f => string.Compare(i, f.Name, true) == 0);

				if (field == null)
					continue;

				DataSource.Fields.Add(CreateField(field.Name, field.DataType));
			}

			DataSource.Parameters.Clear();

			tokens = SelectedParameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var i in tokens)
			{
				var par = Parameters.FirstOrDefault(f => string.Compare(i, f.Name, true) == 0);

				if (par == null)
					continue;

				DataSource.Parameters.Add(CreateParameter(par.Name, par.DataType, par.IsNullable));
			}

			Environment.Commit(DataSource, null, null);

			var r = Result.SectionResult(this, EnvironmentSection.Properties);

			r.Title = SR.DesignerDataSourceImport;
			r.Message = SR.DesignerDataSourceImportDesc;
			r.MessageKind = InformationKind.Success;

			return r;
		}

		protected override void OnInitialize(JObject data)
		{
			_selectedFields = data.Optional("fields", string.Empty);
		}

		public List<ISchemaField> Fields
		{
			get
			{
				if (_fields == null && Browser != null
					&& !string.IsNullOrWhiteSpace(ObjectType) && !string.IsNullOrWhiteSpace(Object))
				{
					try
					{
						_fields = Browser.QuerySchema(ConnectionConfiguration, ObjectType, Object);
					}
					catch { }
				}

				if (_fields == null)
					_fields = new List<ISchemaField>();

				return _fields;
			}
		}
	}
}
