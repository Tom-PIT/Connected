using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Annotations;
using TomPIT.Application.Data;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Design.Designers
{
	public class TransactionDesigner : DataElementDesigner
	{
		public TransactionDesigner(IEnvironment environment, ComponentElement element) : base(environment, element)
		{
		}

		public override string View { get { return "~/Views/Ide/Designers/Transaction.cshtml"; } }

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
					{"parameters", Parameters }
				};

			return Result.JsonResult(this, vb);
		}

		public Transaction Transaction { get { return Owner.Component as Transaction; } }

		protected override IDesignerActionResult ActionImport()
		{
			if (Connection == null)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "connection");

			if (string.IsNullOrWhiteSpace(ObjectType))
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "objectType");

			if (string.IsNullOrWhiteSpace(Object))
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "object");

			var command = Browser.CreateCommandDescriptor(ObjectType, Object);

			Transaction.Connection = DataConnection.Token;
			Transaction.CommandText = command.CommandText;
			Transaction.CommandType = command.CommandType;

			Transaction.Parameters.Clear();

			var tokens = SelectedParameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var i in tokens)
			{
				var par = Parameters.FirstOrDefault(f => string.Compare(i, f.Name, true) == 0);

				if (par == null)
					continue;

				var dataParameter = new Parameter
				{
					DataType = par.DataType,
					Name = par.Name,
					IsNullable = par.IsNullable
				};

				Transaction.Parameters.Add(dataParameter);
			}

			Environment.Commit(Transaction, null, null);

			var r = Result.SectionResult(this, EnvironmentSection.Properties);

			r.Title = SR.DesignerTransactionImport;
			r.Message = SR.DesignerTransactionImportDesc;
			r.MessageKind = InformationKind.Success;

			return r;
		}
	}
}
