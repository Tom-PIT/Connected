using Newtonsoft.Json.Linq;
using System.Linq;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Globalization;
using TomPIT.Design.Ide.Selection;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Environment.Providers;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Runtime;

namespace TomPIT.Ide.Models
{
	public abstract class IdeModelBase : ShellModel, IEnvironment
	{
		private IDom _dom = null;
		private IGlobalizationProvider _globalization = null;
		private ISelectionProvider _selection = null;

		public virtual IDom Dom
		{
			get
			{
				if (_dom == null)
					_dom = CreateDom();

				return _dom;
			}
		}

		protected abstract IDom CreateDom();

		public virtual IGlobalizationProvider Globalization
		{
			get
			{
				if (_globalization == null)
					_globalization = new GlobalizationProvider(this);

				return _globalization;
			}
		}

		public virtual ISelectionProvider Selection
		{
			get
			{
				if (_selection == null)
					_selection = new SelectionProvider(this);

				return _selection;
			}
		}

		public virtual EnvironmentMode Mode { get; } = EnvironmentMode.Design;
		public IMicroServiceContext Context => this;
		public abstract string Id { get; }
		public JObject RequestBody { get; set; }
		public string Path { get; set; }

		public abstract string IdeUrl { get; }

		public virtual IDesignerActionResult Action(JObject data)
		{
			var action = data.Optional("action", string.Empty);

			if (string.IsNullOrWhiteSpace(action))
				throw IdeException.ExpectedParameter(this, 0, "action");

			if (string.Compare(action, "addItem", true) == 0)
				return AddItem(data);
			else if (string.Compare(action, "createItem", true) == 0)
				return CreateItem(data);
			else if (string.Compare(action, "deleteFolder", true) == 0)
				return DeleteFolder(data);
			else if (string.Compare(action, "deleteComponent", true) == 0)
				return DeleteComponent(data);
			else if (string.Compare(action, "move", true) == 0)
				return Move(data);
			else if (string.Compare(action, "clearErrors", true) == 0)
				return ClearErrors(data);

			return Result.EmptyResult(this);
		}

		protected virtual IDesignerActionResult ClearErrors(JObject data)
		{
			return Result.EmptyResult(this);
		}
		protected virtual IDesignerActionResult Move(JObject data)
		{
			return Result.EmptyResult(this);
		}

		protected virtual IDesignerActionResult DeleteFolder(JObject data)
		{
			return Result.EmptyResult(this);
		}

		protected virtual IDesignerActionResult DeleteComponent(JObject data)
		{
			return Result.EmptyResult(this);
		}

		protected virtual IDesignerActionResult AddItem(JObject data)
		{
			var item = data.Required<string>("item");

			return Result.ViewResult(new AddItemModel
			{
				Descriptor = Selection.AddItems.FirstOrDefault(f => string.Compare(f.Id, item, true) == 0),
				Environment = this
			}, "~/Views/Ide/Designers/AddItem.cshtml");
		}

		protected virtual IDesignerActionResult CreateItem(JObject data)
		{
			return Result.EmptyResult(this);
		}
	}
}