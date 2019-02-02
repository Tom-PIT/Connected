using Newtonsoft.Json.Linq;
using System;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Environment;

namespace TomPIT.Dom
{
	public class EnvironmentUnitsElement : TransactionElement
	{
		public const string DomId = "EnvironmentUnits";
		private EnvironmentUnitsDesigner _designer = null;
		private IEnvironmentUnit _selection = null;

		public EnvironmentUnitsElement(IDomElement parent) : base(parent)
		{
			Id = DomId;
			Glyph = "fal fa-folder";
			Title = "Environment units";

			((Behavior)Behavior).AutoExpand = false;
		}

		public override object Component
		{
			get
			{
				if (_selection == null)
				{
					if (!string.IsNullOrWhiteSpace(Environment.Selection.Id))
						_selection = Connection.GetService<IEnvironmentUnitService>().Select(Environment.Selection.Id.AsGuid());
					else
					{
						var mode = Environment.RequestBody.Optional("mode", string.Empty);

						if (string.IsNullOrWhiteSpace(mode))
							return null;

						var id = Environment.RequestBody.Optional("id", Guid.Empty);

						if (id == Guid.Empty)
							return null;

						_selection = Connection.GetService<IEnvironmentUnitService>().Select(id);

						if (_selection != null)
							Environment.Selection.SetId(_selection.Token.ToString());
					}
				}

				return _selection;
			}
		}

		public override bool HasChildren => false;

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new EnvironmentUnitsDesigner(this);

				return _designer;
			}
		}

		public override bool Commit(object component, string property, string attribute)
		{
			var unit = component as IEnvironmentUnit;

			Connection.GetService<IEnvironmentUnitManagementService>().Update(unit.Token, unit.Name, unit.Parent, unit.Ordinal);

			return true;
		}

		public override ITransactionResult Execute(string property, string attribute, string value)
		{
			var selectionId = Environment.RequestBody.Optional("selectionId", Guid.Empty);

			if (selectionId != null)
				Environment.Selection.SetId(selectionId.ToString());

			return base.Execute(property, attribute, value).WithData(new JObject
			{
				{"id", ((IEnvironmentUnit)Component).Token.ToString() }
			});
		}
	}
}
