using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Environment;
using TomPIT.Ide;
using TomPIT.Models;

namespace TomPIT.Designers
{
	public class EnvironmentUnitsDesigner : DomDesigner<EnvironmentUnitsElement>
	{
		private string _propertyView = string.Empty;
		private List<IEnvironmentUnit> _all = null;

		public EnvironmentUnitsDesigner(EnvironmentUnitsElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/EnvironmentUnits.cshtml";

		public List<IEnvironmentUnit> QueryUnits(Guid parent)
		{
			return All.Where(f => f.Parent == parent).OrderBy(f => f.Ordinal).ThenBy(f => f.Name).ToList();
		}

		public override object ViewModel => this;
		public override string PropertyView
		{
			get
			{
				var mode = Environment.RequestBody.Optional("mode", string.Empty);

				if (string.Compare(mode, "addUnit", true) == 0)
					return "~/Views/Ide/Designers/EnvironmentUnitsAddView.cshtml";

				return null;

			}
		}
		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "insert", true) == 0)
			{
				var id = Connection.GetService<IEnvironmentUnitManagementService>().Insert(data.Required<string>("name"),
					data.Optional("parent", Guid.Empty), data.Required<int>("ordinal"));

				return Result.JsonResult(this, new JObject
				{
					{ "id",id.ToString() }
				});
			}
			else if (string.Compare(action, "refresh", true) == 0)
			{
				Environment.Selection.SetId(data.Required<Guid>("id").ToString());

				return Result.ViewResult(new EnvironmentUnitModel(this, Owner.Component as IEnvironmentUnit), "~/Views/Ide/Designers/EnvironmentUnitsNode.cshtml");
			}
			else if (string.Compare(action, "children", true) == 0)
			{
				var items = QueryUnits(data.Required<Guid>("id"));

				return Result.ViewResult(new EnvironmentUnitsModel(this, items), "~/Views/Ide/Designers/EnvironmentUnitsNodes.cshtml");
			}
			else if (string.Compare(action, "delete", true) == 0)
			{
				Connection.GetService<IEnvironmentUnitManagementService>().Delete(data.Required<Guid>("id"));
				return Result.EmptyResult(this);
			}
			else if (string.Compare(action, "move", true) == 0)
			{
				var id = data.Required<Guid>("id");
				var prev = data.Optional("prev", Guid.Empty);
				var parent = data.Optional("parent", Guid.Empty);

				Connection.GetService<IEnvironmentUnitManagementService>().Move(id, prev, parent);

				return Result.EmptyResult(this);
			}

			return base.OnAction(data, action);
		}

		public bool HasChildren(Guid id)
		{
			return QueryUnits(id).Count > 0;
		}

		protected override void OnCreateToolbar(IDesignerToolbar toolbar)
		{
			var ai = new Dropdown(Environment)
			{
				Enabled = true,
				Glyph = "fal fa-plus",
				Id = "AddItems",
				Text = "Add"
			};


			ai.Items.Add(new ToolbarAction(Environment)
			{
				Text = "Sibling",
				Id = "actionAdd"
			});

			ai.Items.Add(new ToolbarAction(Environment)
			{
				Text = "Child",
				Id = "actionAddChild"
			});

			toolbar.Items.Add(ai);

			toolbar.Items.Add(new ToolbarAction(Environment)
			{
				Text = "Delete",
				Id = "actionDelete",
				Glyph = "fal fa-times"
			});
		}

		private List<IEnvironmentUnit> All
		{
			get
			{
				if (_all == null)
					_all = Connection.GetService<IEnvironmentUnitService>().Query();

				return _all;
			}
		}
	}
}
